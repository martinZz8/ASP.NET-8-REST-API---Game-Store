using FluentAssertions;
using GameStore.Web.DbContexts;
using Microsoft.Data.SqlClient;
using GameStore.Web.Services;
using GameStore.Web.Dtos.User;
using System.Net.Http.Headers;
using GameStore.Web.Dtos.Game;
using System.Text;
using Newtonsoft.Json;

// Unit & integration testing based on tutorial: https://www.youtube.com/watch?v=aq3IbO0RwAQ&list=PL82C6-O4XrHeyeJcI5xrywgpfbrqdkQd4&index=1
// Note: Solution for problem with "No test adapters are referenced by this solution" after running tests (install NuGet package " xunit.runner.visualstudio"): https://stackoverflow.com/questions/42714018/why-do-i-get-a-no-test-adapters-are-referenced-by-this-solution-error-message
// Note2: Solution for problem with "Could not find testhost" after running tests (install NuGet package "Microsoft.NET.Test.Sdk"): https://stackoverflow.com/questions/54770830/unable-to-find-testhost-dll-please-publish-your-test-project-and-retry
// Note3: To inject one time "dbContext" together with "sqlConnection", we should use the following tactic with fixture class: https://stackoverflow.com/questions/12976319/xunit-net-global-setup-teardown
// Note4: Reach for the "FluentAssertions" documentation for extension methods: https://fluentassertions.com/introduction
namespace TestGameStore.IntegrationTests
{
    public class GameIntegrationTests : IClassFixture<TestsFixture>
    {
        // URI consts
        private const string API_URI = "https://localhost:7176";
        private const string ADD_GAME_URI = $"{API_URI}/Game";

        // SQL query schemas
        private const string DELETE_GAME_BY_ID_QUERY = "DELETE FROM Games WHERE Id = '{0}';";

        // Db contexts and sql connections
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly SqlConnection _sqlConnection;

        public GameIntegrationTests(TestsFixture testsFixture)
        {
            _applicationDbContext = testsFixture.ApplicationDbContext;
            _sqlConnection = testsFixture.SqlConnection;
        }

        // Note: Naming convention of testing methods: ClassName_MethodName_ExpectedResult
        // Note2: We could also use "Theory" (instead of "Fact") decorator to pass test cases attributes in other decorators like "InlineData" or "MemberData"
        // Note3: Read about difference between "unit tests" and "integration tests": https://stackoverflow.com/questions/62815739/is-it-best-practice-to-test-my-web-api-controllers-directly-or-through-an-http-c
        [Fact]
        public async void GameService_AddGame_AddsGameToDB_IntegrationTest()
        {
            // 1. Arrange - Get your variables and whatever you need before the "Act" state
            using (HttpClient client = new HttpClient())
            {
                // Get JWT of admin and assign it to HTTP client
                string JWT = await GetAdminsJWTToken();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWT);

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
                // based on: https://stackoverflow.com/questions/46044206/c-sharp-body-content-in-post-request
                // for "multipart/form-data" content: https://stackoverflow.com/questions/15176538/net-httpclient-how-to-post-string-value
                var httpContent = new StringContent(JsonConvert.SerializeObject(gameToBeAdded), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(ADD_GAME_URI, httpContent);

                // 3. Assert - Whatever is returned/performed after "Act" state is what you want
                if (!response.IsSuccessStatusCode)
                {
                    Assert.Fail($"Failure during send of HTTP POST request: {await response.Content.ReadAsStringAsync()}");
                }

                // Check if returned DTO object is added properly
                GameDto? returnedGameDto = JsonConvert.DeserializeObject<GameDto>(await response.Content.ReadAsStringAsync());

                if (returnedGameDto == null)
                {
                    Assert.Fail("Returned game DTO is in invalid format. Should match class \"GameDto\"");
                }

                returnedGameDto.Name.Should().Be(gameToBeAdded.Name);
                returnedGameDto.Description.Should().Be(gameToBeAdded.Description);
                returnedGameDto.Price.Should().Be(gameToBeAdded.Price);
                returnedGameDto.OnSale.Should().Be(gameToBeAdded.OnSale);
                returnedGameDto.ReleaseDate.Should().Be(gameToBeAdded.ReleaseDate);
                returnedGameDto.GameGenresDto.Select(it => it.Name).Should().BeEquivalentTo(gameToBeAdded.GameGenreNames);

                // Delete newly created game from database (with game genre connetions)
                KeyValuePair<string, object[]>[] keyValuePairs = new KeyValuePair<string, object[]>[]
                {
                    new KeyValuePair<string, object[]>(DELETE_GAME_BY_ID_QUERY, new object[] {returnedGameDto.Id})
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

        private async Task<string> GetAdminsJWTToken()
        {
            // Create services
            UserService userService = new UserService(_applicationDbContext);

            // Login admin user
            ResultLoginUserDto resultDto = await userService.LoginUser(new LoginUserDto()
            {
                Email = "admin@gmail.com",
                Password = "abc"
            });

            if (resultDto.ErrorDto != null)
            {
                Assert.Fail(resultDto.ErrorDto.Message);
            }

            return resultDto.JWT;
        }
    }
}
