namespace Boycott.Provider {
    using System.Text.RegularExpressions;
    using Boycott.Extensions;

    public class NamingProvider {
        public string GetTableName(string name) {
            return string.Join("_", SplitPascalCase(name)).Pluralize().ToLower();
        }

        public string GetColumnName(string name) {
            return string.Join("_", SplitPascalCase(name)).ToLower();
        }

        public string GetPKColumnName(string name) {
            //return string.Join("_", SplitPascalCase(name)).ToLower().Singularize() + "_id";
            return "id";
        }

        protected string[] SplitCamelCase(string source) {
            return Regex.Split(source, "(?<!^)(?=[A-Z])");
        }

        protected string[] SplitPascalCase(string source) {
            return Regex.Split(source, "(?<!^)(?=[A-Z])");
        }

        internal bool IsPrimaryKey(string name, string typeName) {
            if (name == "Id")
                return true;
            
            if (typeName + "Id" == name)
                return true;
            
            return false;
        }
    }
}
