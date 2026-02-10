using GameStore.Web.Helpers.AppsettingsLoader;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameStore.Web.DbContexts.DesignTime
{
    public class DesignTimeApplicationDbContext : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(AppsettingsLoader.DefaultConnectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
