using MauiSampleApp.Models;

using Microsoft.Extensions.Logging;

namespace MauiSampleApp.Data
{
    public class TaskRepository(ApplicationDbContext context, ILogger<TaskRepository> logger)
    {
        public async Task<List<ProjectTask>> ListAsync() =>
            await context.Tasks.ToListAsync();

        public async Task<List<ProjectTask>> ListAsync(Guid projectId) =>
            await context.Tasks.Where(t => t.ProjectID == projectId).ToListAsync();

        public async Task<ProjectTask?> GetAsync(Guid id) =>
            await context.Tasks.FindAsync(id);

        public async Task<Guid> SaveItemAsync(ProjectTask item)
        {
            if (item.ID == Guid.Empty)
            {
                item.ID = Guid.CreateVersion7();
                await context.Tasks.AddAsync(item);
            }
            else
                context.Tasks.Update(item);

            await context.SaveChangesAsync();
            return item.ID;
        }

        public async Task<int> DeleteItemAsync(ProjectTask item)
        {
            context.Tasks.Remove(item);
            return await context.SaveChangesAsync();
        }

        public async Task DropTableAsync()
        {
            context.Tasks.RemoveRange(context.Tasks);
            await context.SaveChangesAsync();
        }
    }

    public class UserDeviceRepository(ApplicationDbContext context)
    {
        public async Task<List<UserDevice>> ListAsync()
        {
            return await context.UserDevices
                .ToListAsync();
        }

        public async Task<UserDevice?> GetAsync(Guid id)
        {
            return await context.UserDevices
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<List<UserDevice>> GetByApplicationUserIdAsync(Guid applicationUserId)
        {
            return await context.UserDevices
                .Where(p => p.ApplicationUserId == applicationUserId)
                .ToListAsync();
        }

        public async Task<Guid> SaveItemAsync(UserDevice item)
        {
            if (item.ID == Guid.Empty)
            {
                item.ID = Guid.CreateVersion7();
                await context.UserDevices.AddAsync(item);
            }
            else
                context.UserDevices.Update(item);

            await context.SaveChangesAsync();
            return item.ID;
        }

        public async Task<int> DeleteItemAsync(UserDevice item)
        {
            context.UserDevices.Remove(item);
            return await context.SaveChangesAsync();
        }

        public async Task DropTableAsync()
        {
            context.UserDevices.RemoveRange(context.UserDevices);
            await context.SaveChangesAsync();
        }
    }

    public class UserRepository(ApplicationDbContext context)
    {
        public async Task<List<ApplicationUser>> ListAsync()
        {
            return await context.Users
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetAsync(Guid id)
        {
            return await context.Users
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<bool> AnyByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            var normalizedUserName = userName.ToUpper();

            return await context.Users.AnyAsync(u => u.UsernameNormalized == normalizedUserName, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            var normalizedUserName = userName.ToUpper();

            return await context.Users
                .Include(u => u.Devices)
                .FirstOrDefaultAsync(u => u.UsernameNormalized == normalizedUserName, cancellationToken);
        }

        public async Task<Guid> SaveItemAsync(ApplicationUser item)
        {
            if (item.ID == Guid.Empty)
            {
                item.ID = Guid.CreateVersion7();
                await context.Users.AddAsync(item);
            }
            else
                context.Users.Update(item);

            await context.SaveChangesAsync();
            return item.ID;
        }

        public async Task<int> DeleteItemAsync(ApplicationUser item)
        {
            context.Users.Remove(item);
            return await context.SaveChangesAsync();
        }

        public async Task DropTableAsync()
        {
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
        }
    }
}