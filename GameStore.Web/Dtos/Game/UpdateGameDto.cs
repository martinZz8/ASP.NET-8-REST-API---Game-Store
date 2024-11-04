namespace GameStore.Web.Dtos.Game
{
    public record UpdateGameDto
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public decimal Price { get; init; }
        public bool OnSale { get; init; }
        public DateOnly ReleaseDate { get; init; }
        public IEnumerable<string>? GameGenreNames { get; init; }
    }
}
