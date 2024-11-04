using GameStore.Web.Dtos.UserRole;

namespace GameStore.Web.Dtos.User
{
    public record RegisterUserDto
    {
        public string Email { get; init; }
        public string Username { get; init; }
        public string Password { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public IEnumerable<string> Roles { get; init; }
    }
}
