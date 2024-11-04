namespace GameStore.Web.Dtos.GameUserCopy
{
    // It's useful to get this Dto along with Game data
    public record GameUserCopyWithPartialUserDto
    {
        public Guid Id { get; init; }
        public decimal PurchasePrice { get; init; }
        public DateTime PurchaseDate { get; init; }
        public Guid UserId { get; init; }
        public string UserEmail { get; init; }
    }
}
