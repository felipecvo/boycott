namespace Boycott.Migrate {
    using System;

    public class ColumnDefinition {
        public ColumnDefinition() {
            AutoIncrement = false;
        }

        public string Name { get; set; }
        public DbType Type { get; set; }
        public int Limit { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public object Default { get; set; }
        public bool Null { get; set; }
        public bool AutoIncrement { get; set; }

        internal string ToSql() {
            var options = "";
            if (Default != null) {
                options = string.Format(" DEFAULT {0}", Default);
            }
            if (AutoIncrement) {
                options += " AUTO_INCREMENT";
            }
            if (!Null) {
                options += " NOT NULL";
            }
            
            var typeDefinition = Migration.GetType(Type, Limit, Precision, Scale);
            
            return string.Format("{0} {1}{2}", Name, typeDefinition, options);
        }
        
    }
}
