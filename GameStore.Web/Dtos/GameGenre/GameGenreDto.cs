namespace GameStore.Web.Dtos.GameGenre
{
    public record GameGenreDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
    }
}
