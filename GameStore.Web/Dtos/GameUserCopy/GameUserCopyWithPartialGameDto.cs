namespace GameStore.Web.Dtos.GameUserCopy
{
    // It's useful to get this Dto along with User data
    public record GameUserCopyWithPartialGameDto
    {
        public Guid Id { get; init; }
        public decimal PurchasePrice { get; init; }
        public DateTime PurchaseDate { get; init; }
        public Guid GameId { get; init; }
        public string GameName { get; init; }
    }
}
