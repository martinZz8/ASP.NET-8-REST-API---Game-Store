using FluentAssertions;
using GameStore.Web.DbContexts;
using Microsoft.Data.SqlClient;
using GameStore.Web.Services;
using GameStore.Web.Dtos.Game;
using GameStore.Web.Models;

// Unit & integration testing based on tutorial: https://www.youtube.com/watch?v=aq3IbO0RwAQ&list=PL82C6-O4XrHeyeJcI5xrywgpfbrqdkQd4&index=1
// Note: Solution for problem with "No test adapters are referenced by this solution" after running tests (install NuGet package " xunit.runner.visualstudio"): https://stackoverflow.com/questions/42714018/why-do-i-get-a-no-test-adapters-are-referenced-by-this-solution-error-message
// Note2: Solution for problem with "Could not find testhost" after running tests (install NuGet package "Microsoft.NET.Test.Sdk"): https://stackoverflow.com/questions/54770830/unable-to-find-testhost-dll-please-publish-your-test-project-and-retry
// Note3: To inject one time "dbContext" together with "sqlConnection", we should use the following tactic with fixture class: https://stackoverflow.com/questions/12976319/xunit-net-global-setup-teardown
// Note4: Reach for the "FluentAssertions" documentation for extension methods: https://fluentassertions.com/introduction
namespace TestGameStore.UnitTests
{
    public class GameUnitTests : IClassFixture<TestsFixture>
    {
        // SQL query schemas
        private const string DELETE_GAME_BY_ID_QUERY = "DELETE FROM Games WHERE Id = '{0}';";
        private const string GET_GAME_BY_ID_QUERY = "SELECT * FROM Games WHERE Id = '{0}';";
        private const string GET_GAME_GENRE_CONNECTION_BY_ID_QUERY = "SELECT * FROM GameGenres g JOIN GameGenreConnections gc ON gc.GameGenreId = g.Id JOIN Games ga ON gc.GameId = ga.Id AND ga.Id = '{0}';";

        // Db contexts and sql connections
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly SqlConnection _sqlConnection;

        public GameUnitTests(TestsFixture testsFixture)
        {
            _applicationDbContext = testsFixture.ApplicationDbContext;
            _sqlConnection = testsFixture.SqlConnection;
        }

        // Note: Naming convention of testing methods: ClassName_MethodName_ExpectedResult        
        // Note2: We could also use "Theory" (instead of "Fact") decorator to pass test cases attributes in other decorators like "InlineData" or "MemberData"
        // Note3: Read about difference between "unit tests" and "integration tests": https://stackoverflow.com/questions/62815739/is-it-best-practice-to-test-my-web-api-controllers-directly-or-through-an-http-c
        // Note4: We don't need here to authorize user, since unit tests doesn't check authorizartion
        [Fact]
        public async void GameService_AddGame_AddsGameToDB()
        {
            // 1. Arrange - Get your variables and whatever you need before the "Act" state
            // Get "Game" service
            GameService gameService = new GameService(_applicationDbContext);

            // Create new game to be assigned
            AddGameDto gameToBeAdded = new AddGameDto()
            {
                Name = "testName",
                Description = "test description6",
                Price = 200.03M,
                OnSale = true,
                ReleaseDate = new DateOnly(2024, 12, 9),
                GameGenreNames = new string[] { "action" }
            };

            // 2. Act - Execute the methods to perform actions
            // Add new game
            GameDto addedGameDto = await gameService.AddGame(gameToBeAdded);

            // 3. Assert - Whatever is returned/performed after "Act" state is what you want
            // Get the game from DB using SQL query
            Game? foundGame = null;
            List<string> foundGameGenreNames = new List<string>();

            using (SqlCommand command = new SqlCommand(string.Format(GET_GAME_BY_ID_QUERY, addedGameDto.Id), _sqlConnection))
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();

                if (reader.Read())
                {
                    foundGame = new Game();
                    foundGame.Id = (Guid)reader.GetValue(reader.GetOrdinal("Id"));
                    foundGame.Name = (string)reader.GetValue(reader.GetOrdinal("Name"));
                    foundGame.Description = (string)reader.GetValue(reader.GetOrdinal("Description"));
                    foundGame.Price = (decimal)reader.GetValue(reader.GetOrdinal("Price"));
                    foundGame.OnSale = (bool)reader.GetValue(reader.GetOrdinal("OnSale"));
                    foundGame.ReleaseDate = DateOnly.FromDateTime((DateTime)reader.GetValue(reader.GetOrdinal("ReleaseDate")));
                }
            }

            using (SqlCommand command = new SqlCommand(string.Format(GET_GAME_GENRE_CONNECTION_BY_ID_QUERY, addedGameDto.Id), _sqlConnection))
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    foundGameGenreNames.Add((string)reader.GetValue(reader.GetOrdinal("Name")));
                }
            }

            // Check if returned DTO object is added properly
            if (foundGame == null)
            {
                Assert.Fail("Found game DTO is in invalid format. Should match class \"Game\"");
            }

            foundGame.Name.Should().Be(gameToBeAdded.Name);
            foundGame.Description.Should().Be(gameToBeAdded.Description);
            foundGame.Price.Should().Be(gameToBeAdded.Price);
            foundGame.OnSale.Should().Be(gameToBeAdded.OnSale);
            foundGame.ReleaseDate.Should().Be(gameToBeAdded.ReleaseDate);
            foundGameGenreNames.Should().BeEquivalentTo(gameToBeAdded.GameGenreNames);

            // Delete newly created game from database (with game genre connetions)
            KeyValuePair<string, object[]>[] keyValuePairs = new KeyValuePair<string, object[]>[]
            {
                new KeyValuePair<string, object[]>(DELETE_GAME_BY_ID_QUERY, new object[] {foundGame.Id})
            };

            foreach (KeyValuePair<string, object[]> pair in keyValuePairs)
            {
                using (SqlCommand command = new SqlCommand(string.Format(pair.Key, pair.Value), _sqlConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
