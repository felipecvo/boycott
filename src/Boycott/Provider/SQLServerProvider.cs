//using System;
//using Boycott.SqlTranslator;
//using System.Data;
//using System.Linq.Expressions;
//using System.Data.SqlClient;
//using System.Configuration;
//using System.Collections.Generic;

//namespace Boycott.Provider {
//    public class SQLServerProvider : DatabaseProvider {

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
//    }
//}
