using MauiSampleApp.Models;

namespace MauiSampleApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectTask> Tasks { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<ApplicationUser> Users { get; set; } = null!;
        public DbSet<UserDevice> UserDevices { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            //Database.EnsureDeletedAsync().FireAndForgetSafeAsync();
            // Automatically builds the database and schema on mobile devices if it doesn't exist
            Database.EnsureCreatedAsync().FireAndForgetSafeAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.ID);
                entity.Property(c => c.Title).IsRequired();
                entity.Property(c => c.Color).IsRequired();
            });

            // Project Configuration
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.ID);
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.Description).IsRequired();
                entity.Property(p => p.Icon).IsRequired();

                // Many projects to One Category
                entity.HasOne(p => p.Category)
                      .WithMany()
                      .HasForeignKey(p => p.CategoryID)
                      .OnDelete(DeleteBehavior.Restrict);

                // One project to Many Tasks
                entity.HasMany(p => p.Tasks)
                      .WithOne()
                      .HasForeignKey(t => t.ProjectID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Many-to-Many Relationship (Project <-> Tag)
                // Maps cleanly onto your expected SQL join table 'ProjectsTags'
                entity.HasMany(p => p.Tags)
                      .WithMany()
                      .UsingEntity<Dictionary<string, object>>(
                          "ProjectsTags",
                          j => j.HasOne<Tag>().WithMany().HasForeignKey("TagID"),
                          j => j.HasOne<Project>().WithMany().HasForeignKey("ProjectID")
                      );
            });

            // ProjectTask Configuration
            modelBuilder.Entity<ProjectTask>(entity =>
            {
                entity.HasKey(t => t.ID);
                entity.Property(t => t.Title).IsRequired();
                entity.Property(t => t.IsCompleted).IsRequired();
            });

            // Tag Configuration
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(t => t.ID);
                entity.Property(t => t.Title).IsRequired();
                entity.Property(t => t.Color).IsRequired();

                // Ignore UI/Display computed properties so EF Core doesn't try to create columns for them
                entity.Ignore(t => t.ColorBrush);
                entity.Ignore(t => t.DisplayColor);
                entity.Ignore(t => t.DisplayDarkColor);
                entity.Ignore(t => t.DisplayLightColor);
                entity.Ignore(t => t.IsSelected);
            });

            // ApplicationUser Entity Config
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(u => u.ID);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.HasIndex(u => u.UsernameNormalized).IsUnique();
                entity.Property(u => u.UsernameNormalized).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            // UserDevice Entity Config
            modelBuilder.Entity<UserDevice>(entity =>
            {
                entity.HasKey(d => d.ID);
                entity.Property(d => d.DeviceIdentifier).IsRequired();
                entity.Property(d => d.DeviceName).IsRequired();

                // Set up relationship: 1 User -> Many Devices
                entity.HasOne(d => d.User)
                      .WithMany(u => u.Devices)
                      .HasForeignKey(d => d.ApplicationUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}