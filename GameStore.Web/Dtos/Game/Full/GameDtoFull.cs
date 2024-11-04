using GameStore.Web.Dtos.GameGenre;
using GameStore.Web.Dtos.GameUserCopy;

namespace GameStore.Web.Dtos.Game.Full
{
    public record GameDtoFull
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public decimal Price { get; init; }
        public bool OnSale { get; init; }
        public DateOnly ReleaseDate { get; init; }
        public IEnumerable<GameGenreDto> GameGenresDto { get; init; }
        public IEnumerable<GameUserCopyWithPartialUserDto> GameCopiesDto { get; init; }
    }
}
