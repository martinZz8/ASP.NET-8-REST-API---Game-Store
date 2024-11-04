using GameStore.Web.Dtos.UserRole;

namespace GameStore.Web.Dtos.User
{
    public record UserDto
    {
        public Guid Id { get; init; }
        public string Email { get; init; }
        public string Username { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public DateTime? LastLoginDate { get; init; }
        public IEnumerable<UserRoleDto> Roles { get; init; }
    }
}
