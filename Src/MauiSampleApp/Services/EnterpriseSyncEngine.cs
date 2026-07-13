using MauiSampleApp.Models;

using Microsoft.Extensions.Logging;

using System.Net.Http.Json;

namespace MauiSampleApp.Services
{
    public class EnterpriseSyncEngine(
    IServiceScopeFactory scopeFactory,
    IConnectivity connectivity,
    HttpClient httpClient,
    ILogger<EnterpriseSyncEngine> logger) : IEnterpriseSyncEngine
    {
        private readonly SemaphoreSlim _syncLock = new(1, 1);

        private const string LastSyncVersionKey = "Enterprise_LastSyncVersion";

        public void InitializeMonitoring()
        {
            connectivity.ConnectivityChanged += OnNetworkChanged;
            if (connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Task.Run(TriggerSyncAsync);
            }
        }

        public void ShutdownMonitoring()
        {
            connectivity.ConnectivityChanged -= OnNetworkChanged;
        }

        private async void OnNetworkChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                logger.LogInformation("Network connectivity restored. Invoking background execution engine...");
                try
                {
                    await TriggerSyncAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Automated network change background sync cycle failed.");
                }
            }
        }

        public async Task TriggerSyncAsync()
        {
            // Guard against overlapping processing attempts
            if (!await _syncLock.WaitAsync(0))
            {
                logger.LogWarning("Synchronization operation is already active. Skipping loop execution request.");
                return;
            }

            try
            {
                if (connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    logger.LogWarning("Data sync canceled: Device is currently offline.");
                    return;
                }

                logger.LogInformation("Enterprise 2-Way Synchronization cycle processing initiated.");

                // Create a dedicated scope to isolate the DbContext instance cleanly away from UI interactions
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                long currentLocalWatermark = Preferences.Default.Get(LastSyncVersionKey, 0L);

                // 1. Build Payload out of local un-synchronized changes (Pushes up edits and soft-deletes)
                var batchRequest = new SyncBatchRequest
                {
                    LastClientVersion = currentLocalWatermark,
                    Changes = await GatherLocalChangesAsync(context)
                };

                // 2. Dispatch payload via single batch API request
                var response = await httpClient.PostAsJsonAsync("api/v1/sync/process", batchRequest);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("API upstream synchronization returned bad status code: {StatusCode}", response.StatusCode);
                    return;
                }

                var syncResult = await response.Content.ReadFromJsonAsync<SyncBatchResponse>();
                if (syncResult == null) return;

                // 3. Persist incoming updates and clear dirty states within an atomic local database transaction
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    // Acknowledge successfully processed outbound records
                    await ClearDirtyFlagsAsync(context, syncResult.AcknowledgedIds);

                    // Process and apply changes from other clients/devices down to our local store
                    await ApplyRemoteChangesAsync(context, syncResult.ServerChanges);

                    // Commit changes and update our high-watermark synchronization index token
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Preferences.Default.Set(LastSyncVersionKey, syncResult.NewServerVersion);
                    logger.LogInformation("Data sync finalized successfully. Stream version elevated to: {Version}", syncResult.NewServerVersion);
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(dbEx, "Critical error applying sync modifications. SQLite storage state safely rolled back.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal failure caught within the sync execution engine orchestration layer.");
            }
            finally
            {
                _syncLock.Release();
            }
        }

        private async Task<List<SyncItemDto>> GatherLocalChangesAsync(ApplicationDbContext context)
        {
            var localChanges = new List<SyncItemDto>();

            // Gather dirty Project modifications
            var dirtyProjects = await context.Projects.Where(p => p.IsClientDirty).ToListAsync();
            foreach (var proj in dirtyProjects)
            {
                localChanges.Add(new SyncItemDto
                {
                    Id = proj.ID,
                    EntityType = nameof(Project),
                    JsonData = proj.ToJson()!,
                    UpdatedAt = proj.UpdatedAt,
                    IsDeleted = proj.IsDeleted,
                    ServerVersion = proj.ServerVersion
                });
            }

            // Extendable area: Add extra tables right here following the exact same registration pattern
            // var dirtyTasks = await context.Tasks.Where(t => t.IsClientDirty).ToListAsync(); ...

            return localChanges;
        }

        private async Task ClearDirtyFlagsAsync(ApplicationDbContext context, List<Guid> acknowledgedIds)
        {
            if (acknowledgedIds == null || acknowledgedIds.Count == 0) return;

            // Process Projects acknowledged by the server
            var matchedProjects = await context.Projects
                .Where(p => acknowledgedIds.Contains(p.ID))
                .ToListAsync();

            foreach (var proj in matchedProjects)
            {
                if (proj.IsDeleted)
                {
                    // Purge safely out of local cache storage now that server knows it's deleted
                    context.Projects.Remove(proj);
                }
                else
                {
                    proj.IsClientDirty = false;
                }
            }
        }

        private async Task ApplyRemoteChangesAsync(ApplicationDbContext context, List<SyncItemDto> remoteChanges)
        {
            if (remoteChanges == null || remoteChanges.Count == 0) return;

            foreach (var change in remoteChanges)
            {
                if (change.EntityType == nameof(Project))
                {
                    var incomingProject = change.JsonData.FromJson<Project>();
                    if (incomingProject == null) continue;

                    var existingProject = await context.Projects.FindAsync(change.Id);

                    if (existingProject != null)
                    {
                        // --- RESOLVING DATA CONFLICTS (Enterprise Strategy Pattern) ---
                        if (existingProject.IsClientDirty)
                        {
                            // Conflict Scenario: Modified both on server and client while disconnected.
                            // Resolution Strategy: Last-Write-Wins (Compare absolute timestamp indicators)
                            if (change.UpdatedAt > existingProject.UpdatedAt)
                            {
                                logger.LogInformation("Conflict detected for Project {Id}. Server wins due to newer timestamp.", change.Id);
                                OverwriteLocalRecord(context, existingProject, incomingProject, change.ServerVersion);
                            }
                            else
                            {
                                logger.LogInformation("Conflict detected for Project {Id}. Local client modification wins due to newer timestamp.", change.Id);
                                // We do not overwrite. Our local record stays dirty and will be forced up on the next cycle pass.
                            }
                        }
                        else
                        {
                            // Clean Scenario: Standard downstream modification merge
                            OverwriteLocalRecord(context, existingProject, incomingProject, change.ServerVersion);
                        }
                    }
                    else if (!change.IsDeleted)
                    {
                        // Clean Scenario: New entry generated remotely on another platform device
                        incomingProject.IsClientDirty = false;
                        incomingProject.ServerVersion = change.ServerVersion;
                        await context.Projects.AddAsync(incomingProject);
                    }
                }
            }
        }

        private void OverwriteLocalRecord(ApplicationDbContext context, Project local, Project remote, long serverVersion)
        {
            if (remote.IsDeleted)
            {
                context.Projects.Remove(local);
            }
            else
            {
                context.Entry(local).CurrentValues.SetValues(remote);
                local.IsClientDirty = false;
                local.ServerVersion = serverVersion;
            }
        }
    }
}