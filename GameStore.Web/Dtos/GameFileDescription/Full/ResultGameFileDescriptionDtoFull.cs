using GameStore.Web.Dtos.Error;

namespace GameStore.Web.Dtos.GameFileDescription.Full
{
    public record ResultGameFileDescriptionDtoFull
    {
        public ErrorDto? ErrorDto { get; init; } = null;
        public GameFileDescriptionDtoFull? GameFileDescriptionDtoFull { get; init; } = null;
    }
}
