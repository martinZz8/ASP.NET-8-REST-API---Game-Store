using Newtonsoft.Json;

namespace GameStore.Web.Helpers.AppsettingsLoader
{
    public static class AppsettingsLoader
    {
        #region PrivateConsts
        public static readonly string PathToAppsettingsJsonFile = Path.Join(Environment.CurrentDirectory, "appsettings.json");
        #endregion

        #region PrivateStorageOfEnvVariables
        private static string? _loadedJWTSecret; // secret for hashing JWT
        #endregion

        #region PublicGettersOfEnvVariables
        public static string? JWTSecret
        {
            get
            {
                if (_loadedJWTSecret == null)
                {
                    AppsettingsJsonVars? appsettingsJsonVars = LoadAppsettingsFile();
                    if (appsettingsJsonVars != null)
                    {
                        _loadedJWTSecret = appsettingsJsonVars.JWTSecret;
                    }
                }

                return _loadedJWTSecret;
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
            AppsettingsJsonVars? appsettingsJsonVars = LoadAppsettingsFile();
            if (appsettingsJsonVars != null)
            {
                _loadedJWTSecret = appsettingsJsonVars.JWTSecret;
            }            
        }
        #endregion
    }
}
