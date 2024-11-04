using GameStore.Web.Enums;

namespace GameStore.Web.Dtos.Error
{
    public record ErrorExtendedDto: ErrorDto
    {
        public bool IsConflict { get; init; } = true;
    }
}
