namespace Boycott {
    using System;
    using System.Configuration;
    using System.IO;
    using AppConfigEnv;
    using Boycott.Provider;

    static internal class Configuration {
        static Configuration() {
            NamingProvider = GetProvider(new NamingProvider());
            DatabaseProvider = GetDatabaseProvider();
        }

        private static DatabaseProvider GetDatabaseProvider() {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.config");
            if (!File.Exists(path)) {
                path = Path.Combine(Environment.CurrentDirectory, "database.config");
                if (!File.Exists(path)) {
                    throw new Exception("No database.config file found!");
                }
            }
            var config = ConfigFile.Load(path, null);
            
            if (config == null)
                throw new Exception("No database.config file found!");
            
            switch (config["driver"].ToLower()) {
                case "mysql":
                    var p = new MySQLProvider();
                    p.Host = config["host"];
                    p.Database = config["database"];
                    p.User = config["user"];
                    p.Password = config["password"];
                    return p;
                //case "System.Data.SqlClient":
                //default:
                //    return new SQLServerProvider();
            }
            return null;
        }

        public static NamingProvider NamingProvider { get; private set; }

        public static DatabaseProvider DatabaseProvider { get; private set; }

        private static T GetProvider<T>(T defaultProvider) {
            var key = "ActiveRecord.net." + typeof(T).Name;
            var value = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(value)) {
                return (T)Activator.CreateInstance(Type.GetType(value));
            }
            return defaultProvider;
        }
    }
}
