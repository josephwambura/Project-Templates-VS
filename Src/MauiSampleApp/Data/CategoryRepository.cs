using MauiSampleApp.Models;

using Microsoft.Extensions.Logging;

namespace MauiSampleApp.Data
{
    public class CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger)
    {
        public async Task<List<Category>> ListAsync() =>
            await context.Categories.ToListAsync();

        public async Task<Category?> GetAsync(Guid id) =>
            await context.Categories.FindAsync(id);

        public async Task<Guid> SaveItemAsync(Category item)
        {
            if (item.ID == Guid.Empty)
            {
                item.ID = Guid.CreateVersion7();
                await context.Categories.AddAsync(item);
            }
            else
                context.Categories.Update(item);

            await context.SaveChangesAsync();
            return item.ID;
        }

        public async Task<int> DeleteItemAsync(Category item)
        {
            context.Categories.Remove(item);
            return await context.SaveChangesAsync();
        }

        public async Task DropTableAsync()
        {
            context.Categories.RemoveRange(context.Categories);
            await context.SaveChangesAsync();
        }
    }
}