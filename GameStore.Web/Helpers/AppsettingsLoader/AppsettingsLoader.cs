using Newtonsoft.Json;

namespace GameStore.Web.Helpers.AppsettingsLoader
{
    public static class AppsettingsLoader
    {
        #region PrivateConsts
        public static readonly string PathToAppsettingsJsonFile = Path.Join(Environment.CurrentDirectory, "appsettings.json");
        #endregion

        #region PrivateAppsettingsJsonVars
        private static AppsettingsJsonVars? _appsettingsJsonVars;
        #endregion

        #region PublicGettersOfEnvVariables
        public static bool? ApplyMigrationsAtStart
        {
            get
            {
                if (_appsettingsJsonVars == null)
                {
                    _appsettingsJsonVars = LoadAppsettingsFile();
                }

                return _appsettingsJsonVars.ApplyMigrationsAtStart;
            }
        }

        public static bool? ApplyDataInsertionsAtStart
        {
            get
            {
                if (_appsettingsJsonVars == null)
                {
                    _appsettingsJsonVars = LoadAppsettingsFile();
                }

                return _appsettingsJsonVars.ApplyDataInsertionsAtStart;
            }
        }

        public static string? JWTSecret
        {
            get
            {
                if (_appsettingsJsonVars == null)
                {
                    _appsettingsJsonVars = LoadAppsettingsFile();
                }

                return _appsettingsJsonVars.JWTSecret;
            }
        }
        #endregion

        #region PrivateMethods
        private static AppsettingsJsonVars? LoadAppsettingsFile()
        {
            using (StreamReader r = new StreamReader(PathToAppsettingsJsonFile))
            {
                AppsettingsJsonVars? appsettingsJsonVars = JsonConvert.DeserializeObject<AppsettingsJsonVars>(r.ReadToEnd());
                return appsettingsJsonVars;
            }
        }
        #endregion

        #region PublicMethods
        public static void LoadAllEnvVariables()
        {
            _appsettingsJsonVars = LoadAppsettingsFile();            
        }
        #endregion
    }
}
