namespace Boycott.Provider {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text.RegularExpressions;
    using Boycott.Helpers;
    using Boycott.SqlTranslator;
    using MySql.Data.MySqlClient;
    using DbType = Boycott.Migrate.DbType;

    public class MySQLProvider : DatabaseProvider {
        public MySQLProvider() {
            ParameterPrefix = "@";
        }

        public string Host { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public override bool SupportsParameter {
            get { return true; }
        }

        internal override string Quote(string value) {
            return string.Format("`{0}`", value);
        }

        protected override string GetQueryText(SqlQuery query) {
            var sql = query.ToString();
            
            var limit = " LIMIT {0}";
            if (query.Skip > 0) {
                limit = string.Format(limit, query.Skip, "{0}");
                if (query.Take > 0) {
                    limit = string.Format("{0}, {1}", limit, query.Take);
                }
                sql += limit;
            } else if (query.Take > 0) {
                sql += string.Format(limit, string.Format("0, {0}", query.Take));
            }
            
            return sql;
        }

        public override IDbConnection GetConnection() {
            var conn = new MySqlConnection();
            conn.ConnectionString = string.Format("server={0};user id={2}; password={3}; database={1}; pooling=false; Use Procedure Bodies=false;", Host, Database, User, Password);
            conn.Open();
            return conn;
        }

        protected override object ExecuteScalar(SqlQuery query) {
            using (var conn = (MySqlConnection)GetConnection()) {
                var sql = GetQueryText(query);
                var cmd = new MySqlCommand(sql, conn);
                
                foreach (var param in query.Parameters) {
                    cmd.Parameters.AddWithValue(param.Name, param.Value);
                }
                
                return cmd.ExecuteScalar();
            }
        }

        protected override IDataReader ExecuteReader(SqlQuery query) {
            var conn = (MySqlConnection)GetConnection();
            
            var sql = GetQueryText(query);
            var cmd = new MySqlCommand(sql, conn);
            
            foreach (var param in query.Parameters) {
                //object value;
                //var arrayObj = param.Value as Array;
                //if (arrayObj != null) {
                //  var arrayStrings = new string[arrayObj.Length];
                //  for (int i = 0; i < arrayObj.Length; i++) {
                //    arrayStrings[i] = arrayObj.GetValue(i).ToString();
                //  }
                //  value = string.Join(",", arrayStrings);
                //} else {
                //  value = param.Value;
                //}
                //cmd.Parameters.AddWithValue(param.Name, value);
                cmd.Parameters.AddWithValue(param.Name, param.Value);
            }
            
            return cmd.ExecuteReader();
        }

        public override bool SupportsMigrations {
            get { return true; }
        }

        private List<string> tables;
        public override List<string> Tables {
            get {
                if (tables == null) {
                    tables = new List<string>();
                    using (var reader = ExecuteReader("SHOW TABLES;")) {
                        while (reader.Read()) {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
                return tables;
            }
        }

        public override bool DatabaseExists(string databaseName) {
            var conn = new MySqlConnection();
            conn.ConnectionString = string.Format("server={0};user id={2}; password={3}; database={1}; pooling=false; Use Procedure Bodies=false;", Host, databaseName, User, Password);
            try {
                conn.Open();
                return true;
            } catch {
                return false;
            }
        }

        public override void CreateDatabase() {
            var connString = string.Format("server={0};user id={1}; password={2}; pooling=false; Use Procedure Bodies=false;", Host, User, Password);
            
            var conn = new MySqlConnection(connString);
            try {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("CREATE DATABASE {0}", this.Database);
                cmd.ExecuteNonQuery();
            } catch {
            }
        }

        public void DropDatabase() {
            var connString = string.Format("server={0};user id={1}; password={2}; pooling=false; Use Procedure Bodies=false;", Host, User, Password);
            
            var conn = new MySqlConnection(connString);
            try {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("DROP DATABASE {0}", this.Database);
                cmd.ExecuteNonQuery();
            } catch {
            }
        }

        public override List<DbColumn> GetColumns(string tableName) {
            var list = new List<DbColumn>();
            using (var reader = ExecuteReader(string.Format("DESCRIBE {0};", tableName))) {
                while (reader.Read()) {
                    var column = new DbColumn();
                    column.Name = reader.GetString(0);
                    var type = Regex.Match(reader.GetString(1), @"(\w+)\(?(\d+)?,?(\d+)?\)?");
                    column.Type = GetTypeFromDb(type.Groups[1].Value);
                    
                    if (column.Type == Migrate.DbType.String) {
                        column.Limit = int.Parse(type.Groups[2].Value);
                    } else {
                        if (column.Type != DbType.Integer) {
                            if (!string.IsNullOrEmpty(type.Groups[2].Value)) {
                                column.Precision = int.Parse(type.Groups[2].Value);
                            }
                            if (!string.IsNullOrEmpty(type.Groups[3].Value)) {
                                column.Scale = int.Parse(type.Groups[3].Value);
                            }
                        }
                    }
                    if (column.Type == DbType.Short) {
                        if (column.Precision == 1) {
                            column.Type = DbType.Boolean;
                        }
                        column.Precision = 0;
                    }
                    
                    column.Nullable = reader.GetString(2) == "YES";
                    column.IsPrimaryKey = reader.GetString(3) == "PRI";
                    column.DefaultValue = reader.IsDBNull(4) ? null : reader.GetString(4);
                    column.AutoIncrement = reader.GetString(5) == "auto_increment";
                    list.Add(column);
                }
            }
            return list;
        }

        private DbType GetTypeFromDb(string dbType) {
            switch (dbType) {
                case "varchar":
                    return DbType.String;
                case "text":
                    return DbType.Text;
                case "tinyint":
                case "smallint":
                    return DbType.Short;
                case "int":
                    return DbType.Integer;
                case "bigint":
                    return DbType.Long;
                case "float":
                    return DbType.Float;
                case "decimal":
                    return DbType.Decimal;
                case "datetime":
                    return DbType.DateTime;
                case "timestamp":
                    return DbType.Timestamp;
                case "time":
                    return DbType.Time;
                case "date":
                    return DbType.Date;
                case "boolean":
                    return DbType.Boolean;
                default:
                    throw new Exception(string.Format("Type not known: '{0}'", dbType));
            }
        }

        public override string GetAutoIncrement() {
            return "UNIQUE AUTO_INCREMENT";
        }

        public override string GetDbType(DbType type) {
            switch (type) {
                case DbType.String:
                    return "VARCHAR";
                case DbType.Short:
                    return "SMALLINT";
                case DbType.Long:
                    return "BIGINT";
                default:
                    return type.ToString().ToUpper();
            }
        }

        public override void Recycle() {
            tables = null;
        }

        protected override string Escape(string query) {
            return query.Replace(@"\", @"\\");
        }
    }
}
