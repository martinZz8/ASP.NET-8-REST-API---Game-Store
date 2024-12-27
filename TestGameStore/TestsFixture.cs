using GameStore.Web.DbContexts;
using GameStore.Web.Helpers.AppsettingsLoader;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace TestGameStore
{
    public class TestsFixture : IDisposable
    {
        public readonly ApplicationDbContext ApplicationDbContext;
        public readonly SqlConnection SqlConnection;

        public TestsFixture()
        {
            // Do "global" initialization here; Only called once.
            ApplicationDbContext = GetDbContext();
            SqlConnection = GetSqlConnection();
            SqlConnection.Open();
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
            ApplicationDbContext.Dispose();
            SqlConnection.Dispose();
        }

        private ApplicationDbContext GetDbContext()
        {
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(AppsettingsLoader.TestConnectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private SqlConnection GetSqlConnection()
        {
            return new SqlConnection(AppsettingsLoader.TestConnectionString);
        }
    }
}
