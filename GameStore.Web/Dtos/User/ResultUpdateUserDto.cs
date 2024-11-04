using GameStore.Web.Dtos.Error;

namespace GameStore.Web.Dtos.User
{
    public record ResultUpdateUserDto
    {
        public ErrorDto? ErrorDto { get; init; } = null;
        public UserDto? User { get; init; } = null;
    }
}
