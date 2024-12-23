namespace GameStore.Web.Helpers.AppsettingsLoader
{
    public record AppsettingsJsonVars
    {
        public bool ApplyMigrationsAtStart { get; init; }
        public bool ApplyDataInsertionsAtStart { get; init; }
        public string JWTSecret { get; init; }

        public AppsettingsJsonVarsConnectionStrings ConnectionStrings { get; init; }
    }

    public record AppsettingsJsonVarsConnectionStrings
    {
        public string Default { get; init; }
        public string Test { get; init; }
    }
}
