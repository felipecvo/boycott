namespace Boycott.Migrate {
    using System;

    public abstract class Migration {
        internal MigrationProxy Proxy { get; set; }

        public void Migrate(MigrationDirection direction) {
            switch (direction) {
                case MigrationDirection.Up:
                    Announce("migrating");
                    break;
                case MigrationDirection.Down:
                    Announce("reverting");
                    break;
            }
            
            var time = Benchmark.Measure(delegate() {
                if (direction == MigrationDirection.Up) {
                    Up();
                } else {
                    Down();
                }
            });
            
            switch (direction) {
                case MigrationDirection.Up:
                    Announce(string.Format("migrated ({0})", time.Total.TotalSeconds));
                    Write("");
                    break;
                case MigrationDirection.Down:
                    Announce(string.Format("reverted ({0})", time.Total.TotalSeconds));
                    Write("");
                    break;
            }
        }

        public abstract void Up();

        public abstract void Down();

        internal void Announce(string message) {
            var text = string.Format("{0} {1} {2}", Proxy.Version, Proxy.Name, message);
            var length = 75 - text.Length;
            if (length < 0)
                length = 0;
            Write(string.Format("== {0} {1}", text, "".PadLeft(length, '=')));
        }

        internal void Write(string text) {
            Console.WriteLine(text);
        }

        #region Schema Methods

        public static bool TableExists(string tableName) {
            return Configuration.DatabaseProvider.Tables.Contains(tableName);
        }

        public static TableDefinition CreateTable(string tableName) {
            return CreateTable(tableName, new {  });
        }

        public static TableDefinition CreateTable(string tableName, object options) {
            var attrs = AttributesHelper.Parse(options);
            
            var tableDefinition = new TableDefinition(tableName);
            if (attrs.GetValueOrDefault("Id", true)) {
                tableDefinition.PrimaryKey = attrs.GetValueOrDefault("PrimaryKey", Configuration.NamingProvider.GetPKColumnName(tableName));
            }
            
            tableDefinition.Force = attrs.GetValueOrDefault("Force", false);
            tableDefinition.Options = attrs.GetValueOrDefault("Options", string.Empty);
            
            return tableDefinition;
        }

        public static void AddIndex(string tableName, string column, object options) {
            var attrs = AttributesHelper.Parse(options);
            var columnNames = new string[] { column };
            var indexName = IndexName(tableName, columnNames);
            
            var opts = new { Unique = attrs.GetValueOrDefault("Unique", false) ? " UNIQUE" : "", Name = attrs.GetValueOrDefault("Name", indexName), Type = attrs.GetValueOrDefault("Type", string.Empty) };
            
            var query = string.Format("CREATE{0}{1} INDEX {2} ON {3} ({4})", opts.Unique, opts.Type, opts.Name, tableName, string.Join(",", columnNames));
            
            Configuration.DatabaseProvider.ExecuteNonQuery(query);
        }

        private static string IndexName(string tableName, string[] columnNames) {
            return string.Format("idx_{0}_{1}", tableName, string.Join("_", columnNames));
        }

        static internal string GetType(DbType dbType, int limit, int? precision, int? scale) {
            //if native = native_database_types[type]
            //          column_type_sql = (native.is_a?(Hash) ? native[:name] : native).dup
            
            //          if type == :decimal # ignore limit, use precision and scale
            //            scale ||= native[:scale]
            
            //            if precision ||= native[:precision]
            //              if scale
            //                column_type_sql << "(#{precision},#{scale})"
            //              else
            //                column_type_sql << "(#{precision})"
            //              end
            //            elsif scale
            //              raise ArgumentError, "Error adding decimal column: precision cannot be empty if scale if specified"
            //            end
            
            //          elsif (type != :primary_key) && (limit ||= native.is_a?(Hash) && native[:limit])
            //            column_type_sql << "(#{limit})"
            //          end
            
            //          column_type_sql
            //        else
            //          type
            //        end
            var type = "";
            switch (dbType) {
                case DbType.String:
                    type = string.Format("VARCHAR({0})", limit);
                    break;
                case DbType.Long:
                    type = "BIGINT";
                    break;
                case DbType.Decimal:
                case DbType.Float:
                    type = dbType.ToString().ToUpper();
                    if (scale != null) {
                        type += string.Format("({0}, {1})", precision, scale);
                    } else {
                        type += string.Format("({0})", precision);
                    }
                    break;
                default:
                    type = dbType.ToString().ToUpper();
                    break;
            }
            return type;
        }

        public static void DropTable(string tableName) {
            Configuration.DatabaseProvider.ExecuteNonQuery("DROP TABLE " + tableName);
        }

        public void AddColumn(string tableName, string columnName, DbType type) {
            AddColumn(tableName, columnName, type, null);
        }

        public void AddColumn(string tableName, string columnName, DbType type, object options) {
            var attrs = AttributesHelper.Parse(options);
            var opts = new { Default = attrs.GetValueOrDefault("Default", (object)null), Limit = attrs.GetValueOrDefault("Limit", 255), Null = attrs.GetValueOrDefault("Null", true), AutoIncrement = attrs.GetValueOrDefault("AutoIncrement", false), Scale = attrs.GetValueOrDefault("Scale", (int?)null), Precision = attrs.GetValueOrDefault("Precision", (int?)null) };
            
            var column = new ColumnDefinition { Name = columnName, AutoIncrement = opts.AutoIncrement, Default = opts.Default, Limit = opts.Limit, Null = opts.Null, Precision = opts.Precision, Scale = opts.Scale, Type = type };
            
            var sql = string.Format("ALTER TABLE {0} ADD COLUMN {1}", tableName, column.ToSql());
            Console.WriteLine(sql);
            Configuration.DatabaseProvider.ExecuteNonQuery(sql);
        }

        public void RemoveColumn(string tableName, string columnName) {
            Configuration.DatabaseProvider.ExecuteNonQuery(string.Format("ALTER TABLE {0} DROP COLUMN {1}", tableName, columnName));
        }
        
        #endregion
    }

    public class Benchmark {
        public delegate void MeasureMethod();

        public static BenchmarkTime Measure(MeasureMethod method) {
            var time = new BenchmarkTime();
            time.Start = DateTime.Now;
            method();
            time.End = DateTime.Now;
            time.Total = time.End - time.Start;
            return time;
        }
    }

    public class BenchmarkTime {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Total { get; set; }
    }
}
