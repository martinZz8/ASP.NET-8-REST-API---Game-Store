namespace GameStore.Web.Dtos.GameFileDescription
{
    public record GameFileDescriptionDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; init; }
    }
}
