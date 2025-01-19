using GameStore.Web.Dtos.Error;

namespace GameStore.Web.Dtos.XmlProcessor
{
    public record ResultIncrementSongRequestRatingDto
    {
        public ErrorDto? ErrorDto { get; init; } = null;
        public string? SongRequestXml { get; init; }
    }
}
