//using System;
//using Boycott.SqlTranslator;
//using System.Data;
//using System.Linq.Expressions;
//using System.Data.SqlClient;
//using System.Configuration;
//using System.Collections.Generic;

namespace Boycott.Provider {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Boycott.Helpers;
    using Boycott.SqlTranslator;
    using System.Data.SqlClient;
    using DbType = Boycott.Migrate.DbType;
    using System.Text.RegularExpressions;

    public class SQLServerProvider : DatabaseProvider {
        public SQLServerProvider() : this(".", "master", "sa", "sa") { }

        public SQLServerProvider(string dataSource, string database, string user, string password) {
            DataSource = dataSource;
            Database = database;
            User = user;
            Password = password;
        }

        public string DataSource { get; set; }
        public string User { get; set; }
        public string Password { private get; set; }
        //        public SQLServerProvider() {
        //            ParameterPrefix = "@";
        //        }

        //        protected override object ExecuteScalar(SqlQuery query) {
        //            using (var conn = (SqlConnection)GetConnection()) {
        //                var sql = GetQueryText(query);
        //                var cmd = new SqlCommand(sql, conn);

        //                foreach (var param in query.Parameters) {
        //                    cmd.Parameters.AddWithValue(param.Name, param.Value);
        //                }

        //                return cmd.ExecuteScalar();
        //            }
        //        }

        //        protected override IDataReader ExecuteReader(SqlQuery query) {
        //            var conn = (SqlConnection)GetConnection();

        //            var sql = GetQueryText(query);
        //            var cmd = new SqlCommand(sql, conn);

        //            foreach (var param in query.Parameters) {
        //                cmd.Parameters.AddWithValue(param.Name, param.Value);
        //            }

        //            return cmd.ExecuteReader();
        //        }

        //        public override IDbConnection GetConnection() {
        //            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ActiveRecord"].ConnectionString);
        //            conn.Open();
        //            return conn;
        //        }

        //        protected override string GetQueryText(SqlQuery query) {
        //            var sql = query.ToString();

        //            // custom settings
        //            if (query.Skip > 0) {
        //                // Paging
        //                var sql2 = "SELECT * FROM ({0}) AS Limit WHERE {1}";
        //                var rownumber = "SELECT ROW_NUMBER() OVER(ORDER BY {0}) AS Row,";

        //                string where;
        //                if (query.Take > 0) {
        //                    where = string.Format("Row BETWEEN {0} AND {1}", query.Skip + 1, query.Take + query.Skip);
        //                } else {
        //                    where = string.Format("Row > {0}", query.Skip);
        //                }

        //                if (query.OrderBy.Count > 0) {
        //                    var list = new List<string>();
        //                    foreach (var column in query.OrderBy) {
        //                        list.Add(column.ToString());
        //                    }
        //                    rownumber = string.Format(rownumber, string.Join(",", list.ToArray()));
        //                } else {
        //                    rownumber = string.Format(rownumber, "(SELECT 1)");
        //                }
        //                sql = string.Format(sql2, rownumber + sql.Remove(0, 6), where);
        //            } else if (query.Take > 0) {
        //                // top
        //                sql = string.Format("SELECT TOP {0}", query.Take) + sql.Remove(0, 6);
        //            }

        //            return sql;
        //        }

        //        public override bool SupportsParameter {
        //            get { return true; }
        //        }

        //        public override bool SupportsMigrations {
        //            get { return true; }
        //        }
        public override bool SupportsParameter {
            get { return true; }
        }

        public override IDbConnection GetConnection() {
            var conn = new SqlConnection();
            conn.ConnectionString = string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3};", DataSource, Database, User, Password);
            conn.Open();
            return conn;
        }

        public override bool DatabaseExists(string databaseName) {
            var status = true;
            var name = Database;
            Database = databaseName;
            try {
                var conn = (SqlConnection)GetConnection();
                conn.Close();
                SqlConnection.ClearPool(conn);
            } catch (SqlException) {
                status = false;
            }
            Database = name;
            return status;
        }

        public override void CreateDatabase() {
            var name = Database;
            Database = string.Empty;
            using (var conn = GetConnection()) {
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("CREATE DATABASE {0}", name);
                cmd.ExecuteNonQuery();
            }
            Database = name;
        }

        public override void DropDatabase() {
            var name = Database;
            Database = string.Empty;
            using (var conn = GetConnection()) {
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("DROP DATABASE {0}", name);
                cmd.ExecuteNonQuery();
            }
            Database = name;
        }

        private List<string> tables;
        public override List<string> Tables {
            get {
                if (tables == null) {
                    tables = new List<string>();
                    var query = "SELECT name FROM sysobjects WHERE type='U' ORDER BY name";
                    using (var reader = ExecuteReader(query)) {
                        while (reader.Read()) {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
                return tables;
            }
        }

        public override List<DbColumn> GetColumns(string tableName) {
            var list = new List<DbColumn>();
            var pk = (string)ExecuteScalar(string.Format("select 	c.COLUMN_NAME from 	INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk ,INFORMATION_SCHEMA.KEY_COLUMN_USAGE c where pk.TABLE_NAME = '{0}' and	CONSTRAINT_TYPE = 'PRIMARY KEY' and	c.TABLE_NAME = pk.TABLE_NAME and	c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME", tableName));
            using (var reader = ExecuteReader(string.Format("EXEC sp_columns '{0}'", tableName))) {
                while (reader.Read()) {
                    var column = new DbColumn();
                    column.Name = reader.GetString(2);
                    var type = reader.GetString(5);
                    if (type.Contains("identity")) {
                        column.AutoIncrement = true;
                        type = type.Replace("identity", "").Trim();
                    }
                    column.Type = GetTypeFromDb(type);

                    if (column.Type == Migrate.DbType.String || column.Type == Migrate.DbType.Char) {
                        column.Limit = reader.GetInt32(7);
                    } else {
                        if (column.Type != DbType.Integer) {
                            column.Precision = reader.GetInt32(6);
                            if (!reader.IsDBNull(8)) {
                                column.Scale = reader.GetInt32(8);
                            }
                        }
                    }
                    if (column.Type == DbType.Short) {
                        if (column.Precision == 1) {
                            column.Type = DbType.Boolean;
                        }
                        column.Precision = 0;
                    }
                    Console.WriteLine(reader.GetValue(10).GetType().Name);
                    column.Nullable = reader.GetInt16(10) == 1;
                    column.IsPrimaryKey = reader.GetString(3) == "PRI";
                    column.DefaultValue = reader.IsDBNull(12) ? null : reader.GetString(12);
                    if (!string.IsNullOrEmpty(column.DefaultValue) && (column.Type == Migrate.DbType.String || column.Type == Migrate.DbType.Char)) {
                        column.DefaultValue = string.Format("'{0}'", column.DefaultValue);
                    }
                    column.AutoIncrement = reader.GetString(5) == "auto_increment";
                    list.Add(column);
                }
            }
            return list;
        }

        private DbType GetTypeFromDb(string dbType) {
            switch (dbType) {
                case "varchar":
                case "nvarchar":
                case "sysname":
                    return DbType.String;
                case "text":
                case "ntext":
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
                case "bit":
                case "boolean":
                    return DbType.Boolean;
                case "char":
                    return DbType.Char;
                default:
                    throw new Exception(string.Format("Type not known: '{0}'", dbType));
            }
        }

        public override string GetAutoIncrement() {
            return "IDENTITY(1,1)";
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

        protected override IDataReader ExecuteReader(SqlQuery query) {
            var conn = (SqlConnection)GetConnection();

            var sql = GetQueryText(query);
            var cmd = new SqlCommand(sql, conn);

            foreach (var param in query.Parameters) {
                cmd.Parameters.AddWithValue(param.Name, param.Value);
            }

            return cmd.ExecuteReader();
        }

        protected override string GetQueryText(SqlQuery query) {
            var sql = query.ToString();

            // custom settings
            if (query.Skip > 0) {
                // Paging
                var sql2 = "SELECT * FROM ({0}) AS Limit WHERE {1}";
                var rownumber = "SELECT ROW_NUMBER() OVER(ORDER BY {0}) AS Row,";

                string where;
                if (query.Take > 0) {
                    where = string.Format("Row BETWEEN {0} AND {1}", query.Skip + 1, query.Take + query.Skip);
                } else {
                    where = string.Format("Row > {0}", query.Skip);
                }

                if (query.OrderBy.Count > 0) {
                    var list = new List<string>();
                    foreach (var column in query.OrderBy) {
                        list.Add(column.ToString());
                    }
                    rownumber = string.Format(rownumber, string.Join(",", list.ToArray()));
                } else {
                    rownumber = string.Format(rownumber, "(SELECT 1)");
                }
                sql = string.Format(sql2, rownumber + sql.Remove(0, 6), where);
            } else if (query.Take > 0) {
                // top
                sql = string.Format("SELECT TOP {0}", query.Take) + sql.Remove(0, 6);
            }

            return sql;
        }

        protected override object ExecuteScalar(SqlQuery query) {
            using (var conn = (SqlConnection)GetConnection()) {
                var sql = GetQueryText(query);
                var cmd = new SqlCommand(sql, conn);

                foreach (var param in query.Parameters) {
                    cmd.Parameters.AddWithValue(param.Name, param.Value);
                }

                return cmd.ExecuteScalar();
            }
        }

        protected object ExecuteScalar(string query) {
            using (var conn = (SqlConnection)GetConnection()) {
                var cmd = new SqlCommand(query, conn);

                return cmd.ExecuteScalar();
            }
        }

        protected override string Escape(string query) {
            return query;
        }
    }
}
