using GameStore.Web.Dtos.Error;

namespace GameStore.Web.Dtos.User
{
    // Note: We want to handle multiple error messages directly from "UserService", so we return it from this service (and don't create in controller)
    // If "ErrorExtendedDto" is null, we successfully registered user. Otherwise, there's explanation message inside this property (and property in not null).
    public record ResultRegisterUserDto
    {
        public ErrorDto? ErrorDto { get; init; } = null;
    }
}
