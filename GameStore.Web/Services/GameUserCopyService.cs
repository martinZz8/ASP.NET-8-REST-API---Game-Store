using GameStore.Web.Consts;
using GameStore.Web.DbContexts;
using GameStore.Web.Dtos.Error;
using GameStore.Web.Dtos.GameUserCopy;
using GameStore.Web.Enums;
using GameStore.Web.Helpers;
using GameStore.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Web.Services
{
    public interface IGameUserCopyService
    {
        Task<GameUserCopyDto?> AddGameUserCopy(AddGameUserCopy gameUserCopy, Guid currentUserId);
        Task<ResultDeleteGameUserCopyDto> DeleteGameUserCopy(Guid id, Guid currentUserId);
    }

    public class GameUserCopyService: IGameUserCopyService
    {
        private readonly ApplicationDbContext _dbContext;

        public GameUserCopyService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GameUserCopyDto?> AddGameUserCopy(AddGameUserCopy gameUserCopy, Guid currentUserId)
        {
            // Find game with given id
            Game? foundGame = await _dbContext.Games.FirstOrDefaultAsync(it => it.Id.Equals(gameUserCopy.GameId));

            if (foundGame == null)
            {
                return null;
            }

            // Find user with given id
            User? foundUser = await _dbContext.Users.FirstOrDefaultAsync(it => it.Id.Equals(currentUserId));

            if (foundUser == null)
            {
                return null;
            }

            // Create game user copy
            // Note: We don't need to set property "PurchaseDate", because it's setted automatically during addition of this record
            // The code for it is wriiten inside "ApplicationDbContext" class - method "HandleCreationAndUpdateDateForEntities" (first "else if" clause inside "foreach" loop)
            GameUserCopy newGameUserCopy = new GameUserCopy()
            {
                PurchasePrice = foundGame.Price,
                Game = foundGame,
                User = foundUser
            };

            await _dbContext.GameUserCopies.AddAsync(newGameUserCopy);
            await _dbContext.SaveChangesAsync();
            return newGameUserCopy.ToDto();
        }

        public async Task<ResultDeleteGameUserCopyDto> DeleteGameUserCopy(Guid id, Guid currentUserId)
        {
            // Find game user copy with given id
            GameUserCopy? foundGameUserCopy = await _dbContext.GameUserCopies
                .FirstOrDefaultAsync(it => it.Id.Equals(id));

            if (foundGameUserCopy == null)
            {
                return new ResultDeleteGameUserCopyDto()
                {
                    ErrorExtendedDto = new ErrorExtendedDto()
                    {
                        ErrorTypeName = Enum.GetName(ErrorTypeEnum.DELETE),
                        Message = "Error during deletion of game user copy. There doesn't exist game user copy with given id.",
                        IsConflict = false
                    }
                };
            }

            // Check if found game user copy belongs to logged user
            if (!foundGameUserCopy.UserId.Equals(currentUserId))
            {
                // ... if yes, check if user is admin. He should be able to perform this operation. Otherwise return error
                // Get current user roles and check if it's admin
                User? currentUser = await _dbContext.Users
                    .Include(it => it.UserRoleConnections)
                    .ThenInclude(it => it.UserRole)
                    .FirstOrDefaultAsync(it => it.Id.Equals(currentUserId));

                if (currentUser == null || !currentUser.UserRoleConnections.Select(it => it.UserRole.Name).Contains(UserRoleConsts.ADMINISTRATOR))
                {
                    return new ResultDeleteGameUserCopyDto()
                    {
                        ErrorExtendedDto = new ErrorExtendedDto()
                        {
                            ErrorTypeName = Enum.GetName(ErrorTypeEnum.DELETE),
                            Message = "Error during deletion of game user copy. This user copy doesn't belong to currently logged user. Only admin can delete game copies of other user's.",
                            IsConflict = true
                        }
                    };
                }
            }

            // Everything is fine, delete this game user copy
            _dbContext.Remove(foundGameUserCopy);
            await _dbContext.SaveChangesAsync();
            return new ResultDeleteGameUserCopyDto();
        }
    }
}
