using FluentAssertions;
using GameStore.Web.DbContexts;
using Microsoft.Data.SqlClient;
using GameStore.Web.Services;
using GameStore.Web.Dtos.Game;
using GameStore.Web.Models;
using System.Globalization;

// Unit & integration testing based on tutorial: https://www.youtube.com/watch?v=aq3IbO0RwAQ&list=PL82C6-O4XrHeyeJcI5xrywgpfbrqdkQd4&index=1
// Note: Solution for problem with "No test adapters are referenced by this solution" after running tests (install NuGet package " xunit.runner.visualstudio"): https://stackoverflow.com/questions/42714018/why-do-i-get-a-no-test-adapters-are-referenced-by-this-solution-error-message
// Note2: Solution for problem with "Could not find testhost" after running tests (install NuGet package "Microsoft.NET.Test.Sdk"): https://stackoverflow.com/questions/54770830/unable-to-find-testhost-dll-please-publish-your-test-project-and-retry
// Note3: To inject one time "dbContext" together with "sqlConnection", we should use the following tactic with fixture class: https://stackoverflow.com/questions/12976319/xunit-net-global-setup-teardown
// Note4: Reach for the "FluentAssertions" documentation for extension methods: https://fluentassertions.com/introduction
namespace TestGameStore.UnitTests
{
    public class GameUnitTests : IClassFixture<TestsFixture>
    {
        // -- SQL query schemas --
        private const string DELETE_GAME_BY_ID_QUERY = "DELETE FROM Games WHERE Id = '{0}';";
        private const string GET_GAME_BY_ID_QUERY = "SELECT * FROM Games WHERE Id = '{0}';";
        private const string GET_GAME_GENRES_OF_GAME_BY_GAME_ID_QUERY = "SELECT * FROM GameGenres g JOIN GameGenreConnections gc ON gc.GameGenreId = g.Id JOIN Games ga ON gc.GameId = ga.Id AND ga.Id = '{0}';";
        private const string GET_GAME_GENRE_BY_NAME_QUERY = "SELECT * FROM GameGenres WHERE Name = '{0}';";
        private const string ADD_GAME_QUERY = "INSERT INTO Games (Id, Name, Description, Price, OnSale, ReleaseDate, CreateDate, UpdateDate) VALUES ('{0}', '{1}', '{2}', {3}, {4}, '{5}', GETUTCDATE(), GETUTCDATE());";
        private const string ADD_GAME_GENRE_CONNECTION_QUERY = "INSERT INTO GameGenreConnections (Id, GameId, GameGenreId) VALUES (NEWID(), '{0}', '{1}');";

        // -- Db contexts and sql connections --
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly SqlConnection _sqlConnection;

        // - Services --
        private readonly IGameService _gameService;

        // -- Constructors --
        public GameUnitTests(TestsFixture testsFixture)
        {
            _applicationDbContext = testsFixture.ApplicationDbContext;
            _sqlConnection = testsFixture.SqlConnection;
            _gameService = GetGameService();
        }

        // -- Test methods --
        // Note: Naming convention of testing methods: ClassName_MethodName_ExpectedResult        
        // Note2: We could also use "Theory" (instead of "Fact") decorator to pass test cases attributes in other decorators like "InlineData" or "MemberData"
        // Note3: Read about difference between "unit tests" and "integration tests": https://stackoverflow.com/questions/62815739/is-it-best-practice-to-test-my-web-api-controllers-directly-or-through-an-http-c
        // Note4: We don't need here to authorize user, since unit tests doesn't check authorizartion
        [Fact]
        public async void GameService_AddGame_AddsGameToDB()
        {
            // 1. Arrange - Get your variables and whatever you need before the "Act" state
            // -- Nothing to do (creation of object of type "IGameService" moved to constructor) --

            // Create new game to be assigned
            AddGameDto gameToBeAdded = GetRandomGameToBeAdded();

            // 2. Act - Execute the methods to perform actions
            // Add new game
            GameDto? addedGameDto = await _gameService.AddGame(gameToBeAdded);

            // 3. Assert - Whatever is returned/performed after "Act" state is what you want
            // Check if returned "addedGameDto" is null
            addedGameDto.Should().NotBeNull();

            // Get the game from DB using SQL query
            Game? foundGame = await PerformGetGameById(addedGameDto.Id);

            // Get game genre names that belong to added game
            string[] foundGameGenreNames = await PerformGetAllGameGenresOfGameByGameId(addedGameDto.Id);

            // Check if returned "Game" object was added properly
            foundGame.Should().NotBeNull();
            foundGame.Name.Should().Be(gameToBeAdded.Name);
            foundGame.Description.Should().Be(gameToBeAdded.Description);
            foundGame.Price.Should().Be(gameToBeAdded.Price);
            foundGame.OnSale.Should().Be(gameToBeAdded.OnSale);
            foundGame.ReleaseDate.Should().Be(gameToBeAdded.ReleaseDate);
            foundGameGenreNames.Should().BeEquivalentTo(gameToBeAdded.GameGenreNames);

            // 4. Clean
            // Delete newly created game from database (with game genre connetions)
            await PerformDeleteGameByIdSql(foundGame.Id);
        }

        [Fact]
        public async void GameService_GetAllGames_GetAnyGame()
        {
            // 1. Arrange
            // Create random game and add it to DB
            AddGameDto gameToBeAdded = GetRandomGameToBeAdded();
            Guid idOfAddedGame = Guid.NewGuid();
            await PerformAddGameSql(gameToBeAdded, idOfAddedGame);

            // 2. Act
            // Get all games
            IEnumerable<GameDto> gamesDto = (await _gameService.GetAllGames<GameDto>(null, null, null, null, true)).GamesDto;

            // 3. Assert
            // Check if returned at lease one game
            gamesDto.Should().NotBeNullOrEmpty();

            // 4. Clean
            // Delete newly created game from database (with game genre connetions)
            await PerformDeleteGameByIdSql(idOfAddedGame);
        }

        [Fact]
        public async void GameService_GetGameById_GetExistingGame()
        {
            // 1. Arrange
            // Create random game and add it to DB
            AddGameDto gameToBeAdded = GetRandomGameToBeAdded();
            Guid idOfAddedGame = Guid.NewGuid();
            await PerformAddGameSql(gameToBeAdded, idOfAddedGame);

            // 2. Act
            // Get game by id
            GameDto? gameDto = await _gameService.GetGameById<GameDto>(idOfAddedGame);

            // 3. Assert
            // Check if returned game is not null (and if the id equals)
            gameDto.Should().NotBeNull();
            gameDto.Id.Should().Be(idOfAddedGame);
            gameDto.Name.Should().Be(gameToBeAdded.Name);
            gameDto.Description.Should().Be(gameToBeAdded.Description);
            gameDto.Price.Should().Be(gameToBeAdded.Price);
            gameDto.OnSale.Should().Be(gameToBeAdded.OnSale);
            gameDto.ReleaseDate.Should().Be(gameToBeAdded.ReleaseDate);
            gameDto.GameGenresDto.Select(it => it.Name).Should().BeEquivalentTo(gameToBeAdded.GameGenreNames);

            // 4. Clean
            // Delete newly created game from database (with game genre connetions)
            await PerformDeleteGameByIdSql(idOfAddedGame);
        }

        [Fact]
        public async void GameService_UpdateGame_GetUpdatedGame()
        {
            // 1. Arrange
            // Create random game and add it to DB
            AddGameDto gameToBeAdded = GetRandomGameToBeAdded();
            Guid idOfAddedGame = Guid.NewGuid();
            await PerformAddGameSql(gameToBeAdded, idOfAddedGame);

            // 2. Act
            // Update game (locally and in DB) based on it's id
            UpdateGameDto gameToBeUpdated = new UpdateGameDto()
            {
                Name = gameToBeAdded.Name + "_update",
                Description = gameToBeAdded.Description + "_update",
                Price = gameToBeAdded.Price + 10.0M,
                OnSale = !gameToBeAdded.OnSale,
                ReleaseDate = gameToBeAdded.ReleaseDate.AddDays(10),
                GameGenreNames = gameToBeAdded.GameGenreNames?.Append("fantasy").Append("shooter").ToArray()
            };

            GameDto? updatedGameDto = await _gameService.UpdateGame(idOfAddedGame, gameToBeUpdated);

            // 3. Assert
            // Get the game from DB using SQL query
            Game? foundGame = await PerformGetGameById(idOfAddedGame);

            // Get game genre names that belong to updated game
            string[] foundGameGenreNames = await PerformGetAllGameGenresOfGameByGameId(idOfAddedGame);

            // Check if returned "Game" object was updated properly
            foundGame.Should().NotBeNull();
            foundGame.Name.Should().Be(gameToBeUpdated.Name);
            foundGame.Description.Should().Be(gameToBeUpdated.Description);
            foundGame.Price.Should().Be(gameToBeUpdated.Price);
            foundGame.OnSale.Should().Be(gameToBeUpdated.OnSale);
            foundGame.ReleaseDate.Should().Be(gameToBeUpdated.ReleaseDate);
            foundGameGenreNames.Should().BeEquivalentTo(gameToBeUpdated.GameGenreNames);

            // 4. Clean
            // Delete newly created game from database (with game genre connetions)
            await PerformDeleteGameByIdSql(idOfAddedGame);
        }

        [Fact]
        public async void GameService_DeleteGameById_DeletedGame()
        {
            // 1. Arrange
            // Create random game and add it to DB
            AddGameDto gameToBeAdded = GetRandomGameToBeAdded();
            Guid idOfAddedGame = Guid.NewGuid();
            await PerformAddGameSql(gameToBeAdded, idOfAddedGame);

            // 2. Act
            // Delete game by id
            bool result = await _gameService.DeleteGameById(idOfAddedGame);

            // 3. Assert (with cleaning)
            result.Should().BeTrue();

            // Check if game was deleted from db
            Game? foundGame = await PerformGetGameById(idOfAddedGame);

            // Note: We have to clean, only if game wasn't removed successfully from DB
            if (foundGame != null)
            {
                await PerformDeleteGameByIdSql(idOfAddedGame);
            }

            foundGame.Should().BeNull();
        }

        // -- Private methods --
        private IGameService GetGameService()
        {
            return new GameService(_applicationDbContext);
        }

        private AddGameDto GetRandomGameToBeAdded()
        {
            return new AddGameDto()
            {
                Name = "testName",
                Description = "test description6",
                Price = 200.03M,
                OnSale = true,
                ReleaseDate = new DateOnly(2024, 12, 9),
                GameGenreNames = new string[] { "action" }
            };
        }

        // -- Helpful SQL methods --
        private async Task PerformAddGameSql(AddGameDto gameToBeAdded, Guid idOfAddedGame)
        {
            // Add game to DB
            string formattedString = string.Format(ADD_GAME_QUERY, idOfAddedGame.ToString(), gameToBeAdded.Name, gameToBeAdded.Description, gameToBeAdded.Price.ToString(CultureInfo.InvariantCulture), gameToBeAdded.OnSale ? 1 : 0, gameToBeAdded.ReleaseDate.ToString("yyyy-MM-dd"));
            using (SqlCommand command = new SqlCommand(formattedString, _sqlConnection))
            {
                await command.ExecuteNonQueryAsync();
            }

            // Get all game genre ids (which should be connected to new object)
            Guid[] gameGenreIds = await PerformGetAllGameGenresIdsByNames(gameToBeAdded.GameGenreNames.ToArray());

            // Add game genre connections to DB
            foreach (Guid gameGenreId in gameGenreIds)
            {
                using (SqlCommand command = new SqlCommand(string.Format(ADD_GAME_GENRE_CONNECTION_QUERY, idOfAddedGame.ToString(), gameGenreId.ToString()), _sqlConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<Game?> PerformGetGameById(Guid idOfGame)
        {
            Game? foundGame = null;

            using (SqlCommand command = new SqlCommand(string.Format(GET_GAME_BY_ID_QUERY, idOfGame.ToString()), _sqlConnection))
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

            return foundGame;
        }

        private async Task<string[]> PerformGetAllGameGenresOfGameByGameId(Guid gameId)
        {
            List<string> foundGameGenreNames = new List<string>();

            using (SqlCommand command = new SqlCommand(string.Format(GET_GAME_GENRES_OF_GAME_BY_GAME_ID_QUERY, gameId.ToString()), _sqlConnection))
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    foundGameGenreNames.Add((string)reader.GetValue(reader.GetOrdinal("Name")));
                }
            }

            return foundGameGenreNames.ToArray();
        }

        private async Task<Guid?> PerformGetGameGenreIdByName(string gameGenreName)
        {
            using (SqlCommand command = new SqlCommand(string.Format(GET_GAME_GENRE_BY_NAME_QUERY, gameGenreName), _sqlConnection))
            {
                // note: We could also call in here "command.ExecuteScalar()" to get first column of first record returned
                SqlDataReader reader = await command.ExecuteReaderAsync();

                if (reader.Read())
                {
                    return (Guid)reader.GetValue(reader.GetOrdinal("Id"));
                }
            }

            return null;
        }

        private async Task<Guid[]> PerformGetAllGameGenresIdsByNames(string[] gameGenreNames)
        {
            List<Guid> foundGameGenreIds = new List<Guid>();

            foreach (string gameGenreName in gameGenreNames)
            {
                Guid? foundGuid = await PerformGetGameGenreIdByName(gameGenreName);
                foundGuid.Should().NotBeNull($"Couldn't find game genre with name \"{gameGenreName}\"");
                foundGameGenreIds.Add(foundGuid.Value);
            }

            return foundGameGenreIds.ToArray();
        }

        private async Task PerformDeleteGameByIdSql(Guid idOfGame)
        {
            using (SqlCommand command = new SqlCommand(string.Format(DELETE_GAME_BY_ID_QUERY, idOfGame.ToString()), _sqlConnection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
