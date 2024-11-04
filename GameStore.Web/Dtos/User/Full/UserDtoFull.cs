using GameStore.Web.Dtos.GameUserCopy;
using GameStore.Web.Dtos.UserRole;

namespace GameStore.Web.Dtos.User.Full
{
    public record UserDtoFull
    {
        public Guid Id { get; init; }
        public string Email { get; init; }
        public string Username { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public DateTime? LastLoginDate { get; init; }
        public DateTime CreateDate { get; init; }
        public DateTime UpdateDate { get; init; }
        public IEnumerable<GameUserCopyWithPartialGameDto> GameCopiesDto { get; init; }
        public IEnumerable<UserRoleDto> Roles { get; init; }
    }
}
