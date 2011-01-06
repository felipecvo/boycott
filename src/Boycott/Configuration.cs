namespace Boycott {
    using System;
    using System.Configuration;
    using System.IO;
    using Boycott.Provider;

    public static class Configuration {
        static Configuration() {
            NamingProvider = GetProvider(new NamingProvider());
        }

        public static NamingProvider NamingProvider { get; private set; }

        public static DatabaseProvider DatabaseProvider { get; set; }

        private static T GetProvider<T>(T defaultProvider) {
            var key = "Boycott." + typeof(T).Name;
            var value = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(value)) {
                return (T)Activator.CreateInstance(Type.GetType(value));
            }
            return defaultProvider;
        }
    }
}