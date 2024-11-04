using GameStore.Web.Dtos.Error;

namespace GameStore.Web.Dtos.GameUserCopy
{
    public record ResultDeleteGameUserCopyDto
    {
        public ErrorExtendedDto? ErrorExtendedDto { get; init; } = null;
    }
}
