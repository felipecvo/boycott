namespace Boycott.Migrate {
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TableDefinition : IDisposable {
        private bool disposed = false;

        public bool Force { get; set; }
        public string Options { get; set; }
        public string Name { get; set; }
        public string PrimaryKey { get; set; }
        public List<ColumnDefinition> Columns { get; private set; }

        public TableDefinition(string name) {
            Name = name;
            Columns = new List<ColumnDefinition>();
        }

        public void Column(string column_name, DbType type) {
            Column(column_name, type, new object {  });
        }

        public void Column(string column_name, DbType type, object options) {
            var attrs = AttributesHelper.Parse(options);
            var opts = new { Default = attrs.GetValueOrDefault("Default", (string)null), Limit = attrs.GetValueOrDefault("Limit", 255), Precision = attrs.GetValueOrDefault("Precision", (int?)null), Scale = attrs.GetValueOrDefault("Scale", (int?)null), Null = attrs.GetValueOrDefault("Null", true) };
            
            var column = new ColumnDefinition { Name = column_name, Type = type, Default = opts.Default, Limit = opts.Limit, Precision = opts.Precision, Scale = opts.Scale, Null = opts.Null };
            Columns.Add(column);
        }

        public void TimeStamps() {
            Column("created_at", DbType.DateTime);
            Column("updated_at", DbType.DateTime);
        }

        #region IDisposable Members

        public void Dispose() {
            if (!disposed) {
                if (Force && Migration.TableExists(Name)) {
                    Migration.DropTable(Name);
                }
                
                var createSql = new StringBuilder("CREATE TABLE ");
                createSql.Append(Configuration.DatabaseProvider.QuoteTableName(Name));
                createSql.Append(" (");
                createSql.Append(ToSql());
                if (!string.IsNullOrEmpty(PrimaryKey)) {
                    createSql.AppendFormat(", PRIMARY KEY (`{0}`)", PrimaryKey);
                }
                createSql.Append(") ");
                createSql.Append(Options);
                
                Base.Provider.ExecuteNonQuery(createSql.ToString());
                
                disposed = true;
            }
        }

        private string ToSql() {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(PrimaryKey)) {
                list.Add((new ColumnDefinition { Name = PrimaryKey, Null = false, Type = DbType.Integer, AutoIncrement = true }).ToSql());
            }
            foreach (var column in Columns) {
                list.Add(column.ToSql());
            }
            
            return string.Join(",", list.ToArray());
        }

        #endregion

        public void String(string columnName) {
            Column(columnName, DbType.String);
        }

        public void String(string columnName, object options) {
            Column(columnName, DbType.String, options);
        }

        public void Integer(string columnName) {
            Column(columnName, DbType.Integer);
        }

        public void Text(string columnName) {
            Column(columnName, DbType.Text);
        }

        public void Float(string columnName, object options) {
            Column(columnName, DbType.Float, options);
        }

        public void Boolean(string columnName) {
            Column(columnName, DbType.Boolean);
        }

        public void Decimal(string columnName, object options) {
            Column(columnName, DbType.Decimal, options);
        }

        public void DateTime(string columnName) {
            Column(columnName, DbType.DateTime);
        }
    }
}
