using GameStore.Web.Models;
using GameStore.Web.Dtos.GameFileDescription;
using GameStore.Web.Dtos.GameFileDescription.Full;
using System.Text;

namespace GameStore.Web.Helpers
{
    public static class GameFileDescriptionMapper
    {
        // GameFileDescription to GameFileDescriptionDtoFull
        public static GameFileDescriptionDto ToDto(this GameFileDescription game)
        {
            return new GameFileDescriptionDto()
            {
                Id = game.Id,
                GameId = game.GameId
            };
        }

        public static IEnumerable<GameFileDescriptionDto> ToDto(this IEnumerable<GameFileDescription> games)
        {
            return games.Select(x => x.ToDto());
        }

        // GameFileDescription to GameFileDescriptionDtoFull
        public static GameFileDescriptionDtoFull ToDtoFull(this GameFileDescription game)
        {
            return new GameFileDescriptionDtoFull()
            {
                Id = game.Id,
                GameId = game.GameId,
                ContentType = game.ContentType,
                // Conversion taken from: https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
                // Note: here we use conversion to hexadecimal string that represents given byte array (if we use "Encoding.UTF8.GetString", we get the casted characters to string)
                FileBytesString = "0x" + BitConverter.ToString(game.FileData).Replace("-", "")
            };
        }

        public static IEnumerable<GameFileDescriptionDtoFull> ToDtoFull(this IEnumerable<GameFileDescription> games)
        {
            return games.Select(x => x.ToDtoFull());
        }

        // GameFileDescription (generic) to GameFileDescriptionDtoFull or GameFileDescriptionDtoFull
        public static T ToDtoGeneric<T>(this GameFileDescription game) where T : class
        {
            if (typeof(T) == typeof(GameFileDescriptionDto))
            {
                return game.ToDto() as T;
            }
            else if (typeof(T) == typeof(GameFileDescriptionDtoFull))
            {
                return game.ToDtoFull() as T;
            }

            throw new Exception("GameFileDescriptionMapper.ToDtoGeneric(): the given generic class is not proper (available are: 'GameFileDescriptionDto', 'GameFileDescriptionDtoFull')");
        }

        public static IEnumerable<T> ToDtoGeneric<T>(this IEnumerable<GameFileDescription> games) where T : class
        {
            return games.Select(it => it.ToDtoGeneric<T>());
        }
    }
}
