namespace GameStore.Web.Dtos.User
{
    public record UpdateUserDto
    {
        public string Email { get; init; }
        public string Username { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public IEnumerable<string>? Roles { get; init; }
    }
}
