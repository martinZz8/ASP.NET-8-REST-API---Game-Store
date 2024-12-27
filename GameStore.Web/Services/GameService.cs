using GameStore.Web.DbContexts;
using Microsoft.EntityFrameworkCore;
using GameStore.Web.Dtos.Game;
using GameStore.Web.Models;
using GameStore.Web.Helpers;
using GameStore.Web.Dtos.Game.Full;
using GameStore.Web.Dtos.Error;
using GameStore.Web.Enums;

namespace GameStore.Web.Services
{
    public interface IGameService
    {
        Task<ResultGetAllGamesDto<T>> GetAllGames<T>(string? filterGameGenre, string? filterName, bool? filterOnSale, string? orderBy, bool orderAscending) where T: class;
        Task<T?> GetGameById<T>(Guid id) where T : class;
        Task<GameDto?> AddGame(AddGameDto gameDto);
        Task<GameDto?> UpdateGame(Guid id, UpdateGameDto gameDto);
        Task<bool> DeleteGameById(Guid id);
    }

    public class GameService : IGameService
    {
        private readonly ApplicationDbContext _dbContext;

        public GameService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Filtering records by:
        // - game genre (optional),
        // - name (optional, contains insensitive case)
        // - on sale (optional, from query it's string casted to boolean)
        // Add ascending or descending ordering by (use only one):
        // - name (optional),
        // - price (optional),
        // - create date (optional) (ascending is from oldest to newest, descending is from newest to oldest)
        // It uses optional "[FromQuery]" decorators
        // Also add filtering and ordering to metod "GetAllGamesFull"
        // Note: Do filtering on "IQueryable" as long as you can. At the end serialize objects using either "ToListAsync" or "FirstOrDefaultAsync" methods
        // Note2: Use "GameDto" or "GameDtoFull" as generic type "T"
        public async Task<ResultGetAllGamesDto<T>> GetAllGames<T>(string? filterGameGenre, string? filterName, bool? filterOnSale, string? orderBy, bool orderAscending) where T: class
        {
            // Check validity of orderBy argument
            if (!CheckAttributesGameController.CheckOrderBy(orderBy, true))
            {
                return new ResultGetAllGamesDto<T>()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                        Message = $"Error during getting all games. Given 'OrderBy' attribute is invalid. Available are: {String.Join(", ", CheckAttributesGameController.AvailableOrderByNames)}"
                    }
                };
            }

            // Query for gamesQuery with game genres (with "AsNoTracking" to optimize the request, since we don't need to track for changes in entities during GET request)
            IQueryable<Game> gamesQuery = _dbContext.Games
                .Include(it => it.GameGenreConnections)
                .ThenInclude(it => it.GameGenre)
                .AsNoTracking();

            // Chekc if type "T" is "GameDtoFull". If yes, include extra game user copies into query
            if (typeof(T) == typeof(GameDtoFull))
            {
                gamesQuery = gamesQuery
                    .Include(it => it.GameUserCopies)
                    .ThenInclude(it => it.User);
            }

            // Perform filtering (if setted)
            gamesQuery = PerformFiltering(gamesQuery, filterGameGenre, filterName, filterOnSale);

            // Perform ordering (if setted)
            gamesQuery = PerformOrdering(gamesQuery, orderBy, orderAscending);

            // Now we return data after serializing
            // Note: We need to use "where T: class" in the class to let the function "ToDtoGeneric<T>()" work
            return new ResultGetAllGamesDto<T>()
            {
                GamesDto = (await gamesQuery.ToListAsync()).ToDtoGeneric<T>()
            };
        }

        public async Task<T?> GetGameById<T>(Guid id) where T: class
        {
            IQueryable<Game> gameQuery = _dbContext.Games
                .Include(it => it.GameGenreConnections)
                .ThenInclude(it => it.GameGenre)
                .AsNoTracking();

            // Chekc if type "T" is "GameDtoFull". If yes, include extra game user copies into query
            if (typeof(T) == typeof(GameDtoFull))
            {
                gameQuery = gameQuery
                    .Include(it => it.GameUserCopies)
                    .ThenInclude(it => it.User);
            }

            Game? foundGame = await gameQuery.FirstOrDefaultAsync(it => it.Id.Equals(id));
            
            return foundGame?.ToDtoGeneric<T>();
        }

        public async Task<GameDto?> AddGame(AddGameDto gameDto)
        {
            // Find all genres (if any of given is invalid, return null that says about error)
            List<GameGenre> foundGameGenres = new List<GameGenre>();

            if (gameDto.GameGenreNames != null)
            {
                foreach (string gameGenreName in gameDto.GameGenreNames)
                {
                    GameGenre? foundGameGenre = await _dbContext.GameGenres.FirstOrDefaultAsync(it => it.Name.Equals(gameGenreName));

                    if (foundGameGenre == null)
                    {
                        return null;
                    }

                    foundGameGenres.Add(foundGameGenre);
                }
            }

            // Begin database transaction (for game and its gameGenres)
            // based on: https://learn.microsoft.com/en-us/ef/ef6/saving/transactions
            using (var dbContextTransaction = _dbContext.Database.BeginTransaction())
            {
                // Create new game (and save to db)
                // Note: Setting up property "GameGenreConnections = new Collection<GameGenreConnection>()" will not required to check of the nullability of this property in extension method "static GameDto ToDto(this Game game)"
                Game newGame = new Game()
                {
                    Name = gameDto.Name,
                    Description = gameDto.Description,
                    Price = gameDto.Price,
                    OnSale = gameDto.OnSale,
                    ReleaseDate = gameDto.ReleaseDate
                };

                await _dbContext.Games.AddAsync(newGame);
                await _dbContext.SaveChangesAsync();

                // Create GameGenreConnections (and save to db)
                foreach (GameGenre gameGenre in foundGameGenres)
                {
                    GameGenreConnection newGameGenreConnection = new GameGenreConnection()
                    {
                        Game = newGame,
                        GameGenre = gameGenre
                    };

                    _dbContext.GameGenreConnections.Add(newGameGenreConnection);
                }

                await _dbContext.SaveChangesAsync();
                await dbContextTransaction.CommitAsync();
                return newGame.ToDto();
            }
        }

        public async Task<GameDto?> UpdateGame(Guid id, UpdateGameDto gameDto)
        {
            // Check if game with given id exists
            Game? foundGame = await _dbContext.Games
                .Include(it => it.GameGenreConnections)
                .ThenInclude(it => it.GameGenre)
                .FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundGame == null)
            {
                return null;
            }

            // Begin database transaction (for game and its gameGenres)
            // based on: https://learn.microsoft.com/en-us/ef/ef6/saving/transactions
            using (var dbContextTransaction = _dbContext.Database.BeginTransaction())
            {
                // Update game fields (beside the game genre connections)
                foundGame.Name = gameDto.Name;
                foundGame.Description = gameDto.Description;
                foundGame.Price = gameDto.Price;
                foundGame.OnSale = gameDto.OnSale;
                foundGame.ReleaseDate = gameDto.ReleaseDate;

                // Check if "gameDto.GameGenreNames" is not null
                if (gameDto.GameGenreNames != null)
                {
                    // ... if so, remove all connections if "gameDto.GameGenreNames" is empty collection                
                    if (gameDto.GameGenreNames.Count() == 0)
                    {
                        foreach (GameGenreConnection gameGenreConnection in foundGame.GameGenreConnections)
                        {
                            _dbContext.GameGenreConnections.Remove(gameGenreConnection);
                        }
                    }
                    // ... otherwise adjust connections to remove missing ones and add new ones
                    else
                    {
                        // Remove all game genre connections, that are not present in given "gameDto.GameGenreNames"
                        foreach (GameGenreConnection gameGenreConnectionForThisGame in foundGame.GameGenreConnections)
                        {
                            if (!gameDto.GameGenreNames.Contains(gameGenreConnectionForThisGame.GameGenre.Name))
                            {
                                _dbContext.GameGenreConnections.Remove(gameGenreConnectionForThisGame);
                            }
                        }

                        // Add all game genre connections, that are not present in current "gameQuery.GameGenreConnections"
                        IEnumerable<string> currentlyExistingGameConnections = foundGame.GameGenreConnections.Select(it => it.GameGenre.Name);
                        foreach (string gameGenreName in gameDto.GameGenreNames)
                        {
                            if (!currentlyExistingGameConnections.Contains(gameGenreName))
                            {
                                // Find game genre by name (if exists, connect it with game)
                                GameGenre? foundGameGenre = await _dbContext.GameGenres.FirstOrDefaultAsync(it => it.Name.Equals(gameGenreName));

                                if (foundGameGenre == null)
                                {
                                    return null;
                                }

                                GameGenreConnection newGameGenreConnection = new GameGenreConnection()
                                {
                                    Game = foundGame,
                                    GameGenre = foundGameGenre
                                };

                                await _dbContext.GameGenreConnections.AddAsync(newGameGenreConnection);
                            }
                        }
                    }
                }
                // ... otherwise don't touch genre connections for this game

                await _dbContext.SaveChangesAsync();
                await dbContextTransaction.CommitAsync();
                return foundGame.ToDto();
            }
        }

        public async Task<bool> DeleteGameById(Guid id)
        {
            // Note: We could also perform some kind of batch delete (instead of the delete implemented below)
            // We could also intercept the returning value from "ExecuteDeleteAsync" which means the number of deleted rows
            // Based on that, we can return false if count is not equal to 0 (we haven't deleted any rows), otherwise it will be true (return count != 0)
            //await _dbContext.Games.Where(it => it.Id.Equals(id)).ExecuteDeleteAsync();

            Game? foundGame = await _dbContext.Games.FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundGame == null)
            {
                return false;
            }

            _dbContext.Games.Remove(foundGame);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private IQueryable<Game> PerformFiltering(IQueryable<Game> gamesQuery, string? filterGameGenre, string? filterName, bool? filterOnSale)
        {
            if (filterGameGenre != null)
            {
                // Note: we don't need to use "ToLower()" in here, because it by default uses ignore case matching
                gamesQuery = gamesQuery.Where(it => it.GameGenreConnections.Select(it2 => it2.GameGenre.Name.ToLower()).Contains(filterGameGenre.ToLower()));
            }

            if (filterName != null)
            {
                // Note: It can also be: gamesQuery = gamesQuery.Where(it => it.Name.ToLower().IndexOf(filterName.ToLower()) >= 0);
                // Note2: we don't need to use "ToLower()" in here, because it by default uses ignore case matching
                // Note3: function "ToLowerInvariant()" doesn't work with sql queries (we would need to serialize data first, using "ToListAsync()" to let it work
                gamesQuery = gamesQuery.Where(it => it.Name.ToLower().Contains(filterName.ToLower()));
            }

            if (filterOnSale != null)
            {
                gamesQuery = gamesQuery.Where(it => it.OnSale.Equals(filterOnSale));
            }

            return gamesQuery;
        }

        private IQueryable<Game> PerformOrdering(IQueryable<Game> gamesQuery, string? orderBy, bool orderAscending)
        {
            if (orderBy != null)
            {
                switch (orderBy.ToLowerInvariant())
                {
                    case "name":
                        if (orderAscending)
                            gamesQuery = gamesQuery.OrderBy(it => it.Name);
                        else
                            gamesQuery = gamesQuery.OrderByDescending(it => it.Name);
                        break;
                    case "price":
                        if (orderAscending)
                            gamesQuery = gamesQuery.OrderBy(it => it.Price);
                        else
                            gamesQuery = gamesQuery.OrderByDescending(it => it.Price);
                        break;
                    case "createdate":
                        if (orderAscending)
                            gamesQuery = gamesQuery.OrderBy(it => it.CreateDate);
                        else
                            gamesQuery = gamesQuery.OrderByDescending(it => it.CreateDate);
                        break;
                }
            }

            return gamesQuery;
        }
    }
}
