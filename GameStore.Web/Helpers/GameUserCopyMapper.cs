using GameStore.Web.Dtos.GameUserCopy;
using GameStore.Web.Models;

namespace GameStore.Web.Helpers
{
    public static class GameUserCopyMapper
    {
        // GameUserCopy to GameUserCopyDto
        public static GameUserCopyDto ToDto(this GameUserCopy gameUserCopy)
        {
            return new GameUserCopyDto()
            {
                Id = gameUserCopy.Id,
                PurchasePrice = gameUserCopy.PurchasePrice,
                PurchaseDate = gameUserCopy.PurchaseDate,
                GameId = gameUserCopy.GameId,
                UserId = gameUserCopy.UserId
            };
        }

        public static IEnumerable<GameUserCopyDto> ToDto(this IEnumerable<GameUserCopy> gameUserCopies)
        {
            return gameUserCopies.Select(x => x.ToDto());
        }
    }
}
