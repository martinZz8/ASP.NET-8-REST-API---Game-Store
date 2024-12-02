using GameStore.Web.Dtos.Error;

namespace GameStore.Web.Dtos.GameFileDescription.Full
{
    public record ResultGameFileDescriptionDto
    {
        public ErrorDto? ErrorDto { get; init; } = null;
        public GameFileDescriptionDto? GameFileDescriptionDto { get; init; } = null;
    }
}
