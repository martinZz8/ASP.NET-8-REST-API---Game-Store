using GameStore.Web.DbContexts;
using GameStore.Web.Dtos.GameGenre;
using GameStore.Web.Helpers;
using GameStore.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Web.Services
{
    public interface IGameGenreService
    {
        Task<IEnumerable<GameGenreDto>> GetAllGameGenres();
        Task<GameGenreDto?> GetGameGenreById(Guid id);
        Task<GameGenreDto?> GetGameGenreByName(string name);
        Task<GameGenreDto?> AddGameGenre(AddGameGenreDto gameGenreDto);
        Task<GameGenreDto?> UpdateGameGenre(Guid id, UpdateGameGenreDto gameGenreDto);
        Task<bool> DeleteGameGenreById(Guid id);
        Task<bool> DeleteGameGenreByName(string name);
    }

    public class GameGenreService: IGameGenreService
    {
        private readonly ApplicationDbContext _dbContext;

        public GameGenreService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<GameGenreDto>> GetAllGameGenres()
        {
            IEnumerable<GameGenre> gameGenres = await _dbContext.GameGenres.AsNoTracking().ToListAsync();

            return gameGenres.ToDto();
        }

        public async Task<GameGenreDto?> GetGameGenreById(Guid id)
        {
            GameGenre? foundGameGenre = await _dbContext.GameGenres.AsNoTracking().FirstOrDefaultAsync(it => it.Id.Equals(id));

            return foundGameGenre?.ToDto();
        }

        public async Task<GameGenreDto?> GetGameGenreByName(string name)
        {
            GameGenre? foundGameGenre = await _dbContext.GameGenres.AsNoTracking().FirstOrDefaultAsync(it => it.Name.Equals(name));

            return foundGameGenre?.ToDto();
        }

        public async Task<GameGenreDto?> AddGameGenre(AddGameGenreDto gameGenreDto)
        {
            // Find if we currently have game genre that uses given name
            // If yes, return null object (which means error)
            GameGenre? foundGameGenre = await _dbContext.GameGenres.FirstOrDefaultAsync(it => it.Name.Equals(gameGenreDto.Name));

            if (foundGameGenre != null)
            {
                return null;
            }
            // ... otherwise add new game genre
            GameGenre newGameGenre = new GameGenre()
            {
                Name = gameGenreDto.Name
            };

            await _dbContext.AddAsync(newGameGenre);
            await _dbContext.SaveChangesAsync();

            return newGameGenre.ToDto();
        }

        public async Task<GameGenreDto?> UpdateGameGenre(Guid id, UpdateGameGenreDto gameGenreDto)
        {
            // Find if we currently have game genre that uses given name
            // If no, return null object (which means error)
            GameGenre? foundGameGenre = await _dbContext.GameGenres.FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundGameGenre == null)
            {
                return null;
            }
            // ... otherwise update this game genre
            foundGameGenre.Name = gameGenreDto.Name;

            await _dbContext.SaveChangesAsync();
            return foundGameGenre.ToDto();
        }

        public async Task<bool> DeleteGameGenreById(Guid id)
        {
            // Find if we currently have game genre that uses given id
            // If no, return false (which means error)
            GameGenre? foundGameGenre = await _dbContext.GameGenres.FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundGameGenre == null)
            {
                return false;
            }
            // ... otherwise delete found object
            _dbContext.GameGenres.Remove(foundGameGenre);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteGameGenreByName(string name)
        {
            // Find if we currently have game genre that uses given name
            // If no, return false (which means error)
            GameGenre? foundGameGenre = await _dbContext.GameGenres.FirstOrDefaultAsync(it => it.Name.Equals(name));

            if (foundGameGenre == null)
            {
                return false;
            }
            // ... otherwise delete found object
            _dbContext.GameGenres.Remove(foundGameGenre);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
