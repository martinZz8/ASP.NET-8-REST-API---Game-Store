namespace GameStore.Web.Dtos.User
{
    public record SuccessfullyLoggedUserDto
    {
        public string JWT { get; init; }
        public UserDto User { get; init; }
    }
}
