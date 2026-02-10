using GameStore.Web.DbContexts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Web.Helpers
{
    public static class DataExtensions
    {
        // Note: We still need to add new migrations (when data model is changed), but we don't need to apply them manually (as before using "update-database" in PackageManagerConsole)
        public static async Task MigrateToNewestDbAsync(this WebApplication app)
        {
            using IServiceScope scope = app.Services.CreateScope(); // "using" keyword calls the "Dispose" method at the end instead of us
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        // based on: https://stackoverflow.com/questions/21709305/how-to-directly-execute-sql-query-in-c
        // Note: We could also seed data using "OnModelCreating" method inside the "ApplicationDbContext" (modelBuilder.Entity<T>().HasData())
        public static async Task ApplyDataInsertionsAsync(this WebApplication app)
        {
            string connectionString = app.Configuration.GetConnectionString("Default");
            string sqlScriptsPath = Path.Join(Environment.CurrentDirectory, "SqlScripts");
            string[] pathsToSqlFiles = new string[]
            {
                Path.Join(sqlScriptsPath, "InsertInitialGameGenres.sql"),
                Path.Join(sqlScriptsPath, "InsertInitialUserRoles.sql"),
                Path.Join(sqlScriptsPath, "InsertAdminUser.sql"),
                Path.Join(sqlScriptsPath, "InsertNormalUser.sql")
            };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (string pathToSqlFile in pathsToSqlFiles)
                {
                    string queryString = await File.ReadAllTextAsync(pathToSqlFile);

                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        try
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        catch { }
                    }
                }                
            }
        }
    }
}
