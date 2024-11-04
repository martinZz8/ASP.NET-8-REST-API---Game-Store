using GameStore.Web.Dtos.GameUserCopy;
using GameStore.Web.Dtos.User;
using GameStore.Web.Dtos.User.Full;
using GameStore.Web.Models;
using GameStore.Web.Dtos.UserRole;

namespace GameStore.Web.Helpers
{
    public static class UserMapper
    {
        // User to UserDto
        public static UserDto ToDto(this User user)
        {
            return new UserDto()
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                DateOfBirth = user.DateOfBirth,
                LastLoginDate = user.LastLoginDate,
                Roles = user.UserRoleConnections.Select(it => it.UserRole).ToDto()
            };
        }

        public static IEnumerable<UserDto> ToDto(this IEnumerable<User> users)
        {
            return users.Select(it => it.ToDto());
        }

        // User to UserDtoFull
        public static UserDtoFull ToDtoFull(this User user)
        {
            return new UserDtoFull()
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                DateOfBirth = user.DateOfBirth,
                LastLoginDate = user.LastLoginDate,
                CreateDate = user.CreateDate,
                UpdateDate = user.UpdateDate,
                GameCopiesDto = user.GameUserCopies.Select(it => new GameUserCopyWithPartialGameDto()
                {
                    Id = it.Id,
                    PurchasePrice = it.PurchasePrice,
                    PurchaseDate = it.PurchaseDate,
                    GameId = it.GameId,
                    GameName = it.Game.Name
                }),
                Roles = user.UserRoleConnections.Select(it => it.UserRole).ToDto()
            };
        }

        public static IEnumerable<UserDtoFull> ToDtoFull(this IEnumerable<User> users)
        {
            return users.Select(it => it.ToDtoFull());
        }
    }
}
