using GameStore.Web.DbContexts;
using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.GameFileDescription;
using GameStore.Web.Dtos.GameFileDescription.Full;
using GameStore.Web.Enums;
using GameStore.Web.Helpers;
using GameStore.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Web.Services
{
    public interface IGameFileDescriptionService
    {
        Task<IEnumerable<GameFileDescriptionDto>> GetAllGameFileDescriptions();
        Task<IEnumerable<GameFileDescriptionDtoFull>> GetAllGameFileDescriptionsFull(bool useBase64Format);
        Task<GameFileDescriptionDtoFull?> GetGameFileDescriptionById(Guid id, bool useBase64Format);
        Task<ResultGameFileDescriptionDtoFull> GetGameFileDescriptionByGameId(Guid gameId, bool useBase64Format);
        Task<ResultGameFileDescriptionDto> AddGameFileDescription(IFormCollection form);
    }

    public class GameFileDescriptionService : IGameFileDescriptionService
    {
        private readonly ApplicationDbContext _dbContext;

        public GameFileDescriptionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<GameFileDescriptionDto>> GetAllGameFileDescriptions()
        {
            IEnumerable<GameFileDescription> gameFileDescriptions = await _dbContext.GameFileDescriptions.AsNoTracking().ToListAsync();

            return gameFileDescriptions.ToDto();
        }

        public async Task<IEnumerable<GameFileDescriptionDtoFull>> GetAllGameFileDescriptionsFull(bool useBase64Format)
        {
            IEnumerable<GameFileDescription> gameFileDescriptions = await _dbContext.GameFileDescriptions.AsNoTracking().ToListAsync();

            return gameFileDescriptions.ToDtoFull(useBase64Format);
        }

        public async Task<GameFileDescriptionDtoFull?> GetGameFileDescriptionById(Guid id, bool useBase64Format)
        {
            GameFileDescription? foundGameFileDescription = await _dbContext.GameFileDescriptions.AsNoTracking().FirstOrDefaultAsync(it => it.Id.Equals(id));

            return foundGameFileDescription?.ToDtoFull(useBase64Format);
        }

        public async Task<ResultGameFileDescriptionDtoFull> GetGameFileDescriptionByGameId(Guid gameId, bool useBase64Format)
        {
            GameFileDescription? foundGameFileDescription = await _dbContext.GameFileDescriptions.AsNoTracking().FirstOrDefaultAsync(it => it.GameId.Equals(gameId));

            if (foundGameFileDescription == null)
            {
                return new ResultGameFileDescriptionDtoFull()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.READ),
                        Message = "Error during retrieve of game file description. Game with given id doesn't have connected game file description for it."
                    }
                };
            }

            return new ResultGameFileDescriptionDtoFull()
            {
                GameFileDescriptionDtoFull = foundGameFileDescription.ToDtoFull(useBase64Format)
            };
        }

        public async Task<ResultGameFileDescriptionDto> AddGameFileDescription(IFormCollection form)
        {
            // Check if required parameters are given in form
            if (String.IsNullOrEmpty(form["gameId"].ToString()))
            {
                return new ResultGameFileDescriptionDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                        Message = "Error during addition of game file description. Missing form entry with key 'gameId'."
                    }
                };
            }

            if (form.Files.Count != 1)
            {
                return new ResultGameFileDescriptionDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                        Message = $"Error during addition of game file description. Given files entry should have only 1 file (given {form.Files.Count} files)."
                    }
                };
            }

            // Extract params from given form
            Guid gameId = new Guid(form["gameId"]);
            IFormFile file = form.Files.First();

            // Check if file length has 0 bytes (if file is empty)
            if (file?.Length == 0)
            {
                return new ResultGameFileDescriptionDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                        Message = "Error during addition of game file description. The given file length is 0 bytes (file is empty)."
                    }
                };
            }

            // Check if game with given id already exists
            Game? foundGame = await _dbContext.Games
                .Include(it => it.GameFileDescription)
                .AsNoTracking()                
                .FirstOrDefaultAsync(it => it.Id.Equals(gameId));

            if (foundGame == null)
            {
                return new ResultGameFileDescriptionDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                        Message = "Error during addition of game file description. Game with given id doesn't exist."
                    }
                };
            }

            // Check if game has already assigned "GameFileDescription" object
            if (foundGame.GameFileDescription != null)
            {
                return new ResultGameFileDescriptionDto()
                {
                    ErrorDto = new ErrorDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.CREATE),
                        Message = "Error during addition of game file description. Game already contains attached 'GameFileDescription'."
                    }
                };
            }

            // If validation is performed, attach file to found game (by creating new "GameFileDescription" object)
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();

                // Note: There is error when I want to add "Game" parameter (duplicated id of game, even if it's not for the "GameFileDescriptions" table). Instead I need to pass "GameId"
                GameFileDescription gameFileDescription = new GameFileDescription()
                {
                    FileName = file.Name,
                    FileData = fileBytes,
                    ContentType = file.ContentType,
                    GameId = gameId
                };

                await _dbContext.GameFileDescriptions.AddAsync(gameFileDescription);
                await _dbContext.SaveChangesAsync();

                return new ResultGameFileDescriptionDto()
                {
                    GameFileDescriptionDto = gameFileDescription.ToDto()
                };
            }
        }
    }
}
