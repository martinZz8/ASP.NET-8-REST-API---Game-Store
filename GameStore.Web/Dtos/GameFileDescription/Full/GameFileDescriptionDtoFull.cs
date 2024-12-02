namespace GameStore.Web.Dtos.GameFileDescription.Full
{
    public record GameFileDescriptionDtoFull
    {
        public Guid Id { get; set; }
        public Guid GameId { get; init; }
        public string ContentType { get; init; }
        public string FileBytesString { get; init; }
    }
}
