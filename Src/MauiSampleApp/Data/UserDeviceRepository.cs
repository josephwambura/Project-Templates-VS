using MauiSampleApp.Models;

namespace MauiSampleApp.Data
{
    public class UserDeviceRepository(ApplicationDbContext context)
    {
        public async Task<List<UserDevice>> ListAsync()
        {
            return await context.UserDevices
                .ToListAsync();
        }

        public async Task<List<UserDevice>> ListByApplicationUserIdAsync(Guid applicationUserId)
        {
            return await context.UserDevices
                .Where(p => p.ApplicationUserId == applicationUserId)
                .ToListAsync();
        }

        public async Task<UserDevice?> GetAsync(Guid id)
        {
            return await context.UserDevices
                .FirstOrDefaultAsync(p => p.ID == id);
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
}