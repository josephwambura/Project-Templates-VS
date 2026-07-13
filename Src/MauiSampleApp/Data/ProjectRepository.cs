using MauiSampleApp.Models;

using Microsoft.Extensions.Logging;

namespace MauiSampleApp.Data
{
    public class ProjectRepository(ApplicationDbContext context, ILogger<ProjectRepository> logger)
    {
        public async Task<List<Project>> ListAsync()
        {
            return await context.Projects
                .Include(p => p.Category)
                .Include(p => p.Tasks)
                .Include(p => p.Tags)
                .ToListAsync();
        }

        public async Task<Project?> GetAsync(Guid id)
        {
            return await context.Projects
                .Include(p => p.Category)
                .Include(p => p.Tasks)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<Guid> SaveItemAsync(Project item)
        {
            if (item.ID == Guid.Empty)
            {
                item.ID = Guid.CreateVersion7();
                await context.Projects.AddAsync(item);
            }
            else
                context.Projects.Update(item);

            await context.SaveChangesAsync();
            return item.ID;
        }

        public async Task<int> DeleteItemAsync(Project item)
        {
            context.Projects.Remove(item);
            return await context.SaveChangesAsync();
        }

        public async Task DropTableAsync()
        {
            context.Projects.RemoveRange(context.Projects);
            context.Tasks.RemoveRange(context.Tasks);
            context.Tags.RemoveRange(context.Tags);
            await context.SaveChangesAsync();
        }
    }
}