using GameStore.Web.Dtos.Game;
using GameStore.Web.Dtos.Game.Full;
using GameStore.Web.Dtos.GameGenre;
using GameStore.Web.Dtos.GameUserCopy;
using GameStore.Web.Models;

namespace GameStore.Web.Helpers
{
    public static class GameMapper
    {
        // Game to GameDto
        public static GameDto ToDto(this Game game)
        {
            // Note: This way of setting "GameGenresDto" (with checking of nullability of collection "game.GameGenreConnections") is for POST games (add new game) service method.
            return new GameDto()
            {
                Id = game.Id,
                Name = game.Name,
                Description = game.Description,
                OnSale = game.OnSale,
                Price = game.Price,
                ReleaseDate = game.ReleaseDate,
                GameGenresDto = game.GameGenreConnections != null ?
                    game.GameGenreConnections.Select(it => it.GameGenre.ToDto())
                :
                    Enumerable.Empty<GameGenreDto>()
            };
        }

        public static IEnumerable<GameDto> ToDto(this IEnumerable<Game> games)
        {
            return games.Select(x => x.ToDto());
        }

        // Game to GameDtoFull
        public static GameDtoFull ToDtoFull(this Game game)
        {
            // Note: This way of setting "GameGenresDto" (with checking of nullability of collection "game.GameGenreConnections") is for POST games (add new game) service method.
            return new GameDtoFull()
            {
                Id = game.Id,
                Name = game.Name,
                Description = game.Description,
                OnSale = game.OnSale,
                Price = game.Price,
                ReleaseDate = game.ReleaseDate,
                GameGenresDto = game.GameGenreConnections != null ?
                    game.GameGenreConnections.Select(it => it.GameGenre.ToDto())
                :
                    Enumerable.Empty<GameGenreDto>(),
                GameCopiesDto = game.GameUserCopies.Select(it => new GameUserCopyWithPartialUserDto()
                {
                    Id = it.Id,
                    PurchasePrice = it.PurchasePrice,
                    PurchaseDate = it.PurchaseDate,
                    UserId = it.UserId,
                    UserEmail = it.User.Email
                })
            };
        }

        public static IEnumerable<GameDtoFull> ToDtoFull(this IEnumerable<Game> games)
        {
            return games.Select(x => x.ToDtoFull());
        }

        // Game (generic) to GameDto or GameDtoFull
        public static T ToDtoGeneric<T>(this Game game) where T: class
        {
            if (typeof(T) == typeof(GameDto))
            {
                return game.ToDto() as T;
            }
            else if (typeof(T) == typeof(GameDtoFull))
            {
                return game.ToDtoFull() as T;
            }

            throw new Exception("GameMapper.ToDtoGeneric(): the given generic class is not proper (available are: 'GameDto', 'GameDtoFull')");
        }

        public static IEnumerable<T> ToDtoGeneric<T>(this IEnumerable<Game> games) where T : class
        {
            return games.Select(it => it.ToDtoGeneric<T>());
        }
    }
}
