namespace GameStore.Web.Dtos.Game
{
    // Note:
    // 1) RRS (Readonly Record Struct) is faster than normal class or record.
    // It's because it's pure value type element
    // Use it like: "public readonly record struct AddGameDto"
    // 2) Probaly is faster to use "ICollection" rather than "IEnumerable" (e.g. for "GameGenreNames")
    public record AddGameDto
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public decimal Price { get; init; }
        public bool OnSale { get; init; }
        public DateOnly ReleaseDate { get; init; }
        public IEnumerable<string>? GameGenreNames { get; init; }
    }
}
