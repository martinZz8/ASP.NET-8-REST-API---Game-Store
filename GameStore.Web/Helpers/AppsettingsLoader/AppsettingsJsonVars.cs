namespace GameStore.Web.Helpers.AppsettingsLoader
{
    public record AppsettingsJsonVars
    {
        public bool ApplyMigrationsAtStart { get; init; }
        public bool ApplyDataInsertionsAtStart { get; init; }
        public string JWTSecret { get; init; }
    }
}
