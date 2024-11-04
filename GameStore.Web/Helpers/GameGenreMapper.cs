using GameStore.Web.Dtos.GameGenre;
using GameStore.Web.Models;

namespace GameStore.Web.Helpers
{
    public static class GameGenreMapper
    {
        // GameGenre to GameGenreDto
        public static GameGenreDto ToDto(this GameGenre gameGenre)
        {
            return new GameGenreDto()
            {
                Id = gameGenre.Id,
                Name = gameGenre.Name
            };
        }

        public static IEnumerable<GameGenreDto> ToDto(this IEnumerable<GameGenre> gameGenres)
        {
            return gameGenres.Select(x => x.ToDto());
        }
    }
}
