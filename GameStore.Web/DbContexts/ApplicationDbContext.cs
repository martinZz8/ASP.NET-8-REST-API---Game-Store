using GameStore.Web.Models;
using GameStore.Web.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GameStore.Web.DbContexts
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {}

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserUserRoleConnection> UserUserRoleConnections { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameGenre> GameGenres { get; set; }
        public DbSet<GameGenreConnection> GameGenreConnections { get; set; }
        public DbSet<GameUserCopy> GameUserCopies { get; set; }        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // -- Write some options of entity creations default SQL values --
            // ** User **
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.DateOfBirth)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(u => u.LastLoginDate)
                .IsRequired(false);

            //modelBuilder.Entity<User>()
            //    .Property(u => u.CreateDate)
            //    .HasColumnType("datetime2");

            //modelBuilder.Entity<User>()
            //    .Property(u => u.CreateDate)
            //    .HasDefaultValueSql("getutcdate()"); //getdate()

            // ** UserRole **
            modelBuilder.Entity<UserRole>()
                .HasIndex(u => u.Name)
                .IsUnique();

            // ** Game **
            modelBuilder.Entity<Game>()
                .Property(u => u.Description)
                .IsRequired(false);

            modelBuilder.Entity<Game>()
                .Property(u => u.Price)
                .HasColumnType("decimal(18,2)");

            // ** GameGenre **
            modelBuilder.Entity<GameGenre>()
                .HasIndex(u => u.Name)
                .IsUnique();

            // ** GameUserCopy **
            modelBuilder.Entity<GameUserCopy>()
                .Property(u => u.PurchasePrice)
                .HasColumnType("decimal(18,2)");

            base.OnModelCreating(modelBuilder);
        }

        // Auto fill of entities "CreationDate" and "UpdateDate" (after creation or update of any entit)
        // based on: https://stackoverflow.com/questions/37285948/how-to-set-created-date-and-modified-date-to-enitites-in-db-first-approach
        public override int SaveChanges()
        {
            HandleCreationAndUpdateDateForEntities();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            HandleCreationAndUpdateDateForEntities();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void HandleCreationAndUpdateDateForEntities()
        {
            DateTime nowDate = DateTime.UtcNow;

            foreach (EntityEntry changedEntity in ChangeTracker.Entries())
            {
                if (changedEntity.Entity is IEntityWithDates entity)
                {
                    switch (changedEntity.State)
                    {
                        case EntityState.Added:
                            entity.CreateDate = nowDate;
                            entity.UpdateDate = nowDate;
                            break;
                        case EntityState.Modified:
                            Entry(entity).Property(x => x.CreateDate).IsModified = false;
                            entity.UpdateDate = nowDate;
                            break;
                    }
                }
                else if (changedEntity.Entity is GameUserCopy entity2)
                {
                    switch (changedEntity.State)
                    {
                        case EntityState.Added:
                            entity2.PurchaseDate = nowDate;
                            break;
                    }
                }
            }
        }
    }
}
