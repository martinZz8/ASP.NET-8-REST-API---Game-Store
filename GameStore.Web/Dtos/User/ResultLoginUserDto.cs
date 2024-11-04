using GameStore.Web.Dtos.Error;

namespace GameStore.Web.Dtos.User
{
    // Note: We could handle this dto without inner "ErrorExtendedDto" property, but we want to return detailed result of why the login failer (wrong email or password) from service.
    // Inside controller, we return overall information whether the login succeeded or failed (without detailed informations).
    // If succeeded, we fill JWT, otherwise we fill "ErrorExtendedDto".
    public record ResultLoginUserDto
    {
        public ErrorDto? ErrorDto { get; init; } = null;
        public string? JWT { get; init; } = null;
        public UserDto? User { get; init; } = null;
    }
}
