namespace Boycott.SqlTranslator {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SqlWhereConstant : SqlExpression {
        public object Value { get; set; }
        public override string ToString() {
            if (Value is Array) {
                var arg = (Array)Value;
                var list = new List<string>(arg.Length);
                foreach (var item in arg) {
                    var sql = new SqlWhereConstant { Value = item };
                    list.Add(sql.ToString());
                }
                return string.Format("({0})", string.Join(",", list.ToArray()));
            }
            return Configuration.DatabaseProvider.GetDBValue(Value);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is SqlWhereConstant))
                return false;
            var sql = (SqlWhereConstant)obj;
            var @equals = Value.Equals(sql.Value);
            return @equals;
        }

        public override string ToParametrizedString(SqlParameterCollection list) {
            if (Value is Array)
                return ToString();
            
            var parameter = list.AddOrGet(Value);
            return parameter.ToString();
        }
    }
}
