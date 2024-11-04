using GameStore.Web.Dtos.Error;

namespace GameStore.Web.Dtos.Game
{
    public record ResultGetAllGamesDto<T>
    {
        public ErrorDto? ErrorDto { get; init; } = null;
        public IEnumerable<T> GamesDto { get; init; }
    }
}
