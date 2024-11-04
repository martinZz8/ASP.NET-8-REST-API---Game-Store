namespace GameStore.Web.Dtos.GameUserCopy
{
    public record GameUserCopyDto
    {
        public Guid Id { get; init; }
        public decimal PurchasePrice { get; init; }
        public DateTime PurchaseDate { get; init; }
        public Guid GameId { get; init; }
        public Guid UserId { get; init; }
    }
}
