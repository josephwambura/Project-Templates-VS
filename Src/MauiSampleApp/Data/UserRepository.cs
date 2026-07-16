using MauiSampleApp.Models;

namespace MauiSampleApp.Data
{
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