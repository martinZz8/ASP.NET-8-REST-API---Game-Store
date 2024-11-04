using GameStore.Web.Dtos.GameGenre;

namespace GameStore.Web.Dtos.Game
{
    public record GameDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public decimal Price { get; init; }
        public bool OnSale { get; init; }
        public DateOnly ReleaseDate { get; init; }
        public IEnumerable<GameGenreDto> GameGenresDto { get; init; }
    }
}
