using Newtonsoft.Json;

namespace GameStore.Web.Helpers.AppsettingsLoader
{
    public static class AppsettingsLoader
    {
        #region PrivateConsts
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

        public static string? DefaultConnectionString
        {
            get
            {
                if (_appsettingsJsonVars == null)
                {
                    _appsettingsJsonVars = LoadAppsettingsFile();
                }

                return _appsettingsJsonVars.ConnectionStrings.Default;
            }
        }

        public static string? TestConnectionString
        {
            get
            {
                if (_appsettingsJsonVars == null)
                {
                    _appsettingsJsonVars = LoadAppsettingsFile();
                }

                return _appsettingsJsonVars.ConnectionStrings.Test;
            }
        }
        #endregion

        #region PrivateMethods
        private static AppsettingsJsonVars? LoadAppsettingsFile(string envName = "Development")
        {
            string appsettingsFilePath = Path.Join(Environment.CurrentDirectory, $"appsettings.{envName}.json");

            using (StreamReader r = new StreamReader(appsettingsFilePath))
            {
                AppsettingsJsonVars? appsettingsJsonVars = JsonConvert.DeserializeObject<AppsettingsJsonVars>(r.ReadToEnd());
                return appsettingsJsonVars;
            }
        }
        #endregion

        #region PublicMethods
        public static void LoadAllEnvVariables(string envName)
        {
            _appsettingsJsonVars = LoadAppsettingsFile(envName);
        }
        #endregion
    }
}
