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
}