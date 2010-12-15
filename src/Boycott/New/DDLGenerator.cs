namespace Boycott.New {
    using System.Collections.Generic;

    public class DDLGenerator {
        public string CreateDatabase(string name, object options) {
            return string.Format("CREATE DATABASE {0}", name);
        }

        public string DropDatabase(string name, object options) {
            return string.Format("DROP DATABASE {0}", name);
        }

        public string CreateTable(string name, Dictionary<string, string> columns) {
            return null;
        }
    }
}
