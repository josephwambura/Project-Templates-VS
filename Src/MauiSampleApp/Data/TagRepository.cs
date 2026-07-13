using MauiSampleApp.Models;

using Microsoft.Extensions.Logging;

namespace MauiSampleApp.Data
{
    public class TagRepository(ApplicationDbContext context, ILogger<TagRepository> logger)
    {
        public async Task<List<Tag>> ListAsync() =>
            await context.Tags.ToListAsync();

        public async Task<List<Tag>> ListAsync(Guid projectID)
        {
            var project = await context.Projects
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.ID == projectID);

            return project?.Tags ?? [];
        }

        public async Task<Tag?> GetAsync(Guid id) =>
            await context.Tags.FindAsync(id);

        public async Task<Guid> SaveItemAsync(Tag item)
        {
            if (item.ID == Guid.Empty)
            {
                item.ID = Guid.CreateVersion7();
                await context.Tags.AddAsync(item);
            }
            else
                context.Tags.Update(item);

            await context.SaveChangesAsync();
            return item.ID;
        }

        public async Task<int> SaveItemAsync(Tag item, Guid projectID)
        {
            await SaveItemAsync(item);

            var project = await context.Projects
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.ID == projectID);

            if (project != null && !project.Tags.Any(t => t.ID == item.ID))
            {
                project.Tags.Add(item);
                await context.SaveChangesAsync();
                return 1;
            }
            return 0;
        }

        public async Task<int> DeleteItemAsync(Tag item)
        {
            context.Tags.Remove(item);
            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteItemAsync(Tag item, Guid projectID)
        {
            var project = await context.Projects
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.ID == projectID);

            if (project != null)
            {
                var tagToRemove = project.Tags.FirstOrDefault(t => t.ID == item.ID);
                if (tagToRemove != null)
                {
                    project.Tags.Remove(tagToRemove);
                    return await context.SaveChangesAsync();
                }
            }
            return 0;
        }

        public async Task DropTableAsync()
        {
            context.Tags.RemoveRange(context.Tags);
            await context.SaveChangesAsync();
        }
    }
}