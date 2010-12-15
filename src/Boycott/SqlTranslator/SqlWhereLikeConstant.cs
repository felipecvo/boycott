namespace Boycott.SqlTranslator {
    using System;
    using System.Text;

    public class SqlWhereLikeConstant : SqlExpression {
        public string Value { get; set; }
        public SqlLikeType Type { get; set; }

        public override int GetHashCode() {
            return Value.GetHashCode() + Type.GetHashCode();
        }

        public override bool Equals(object obj) {
            var sql = (SqlWhereLikeConstant)obj;
            var @equals = Value.Equals(sql.Value);
            @equals &= Type.Equals(sql.Type);
            return @equals;
        }

        public override string ToParametrizedString(SqlParameterCollection list) {
            var builder = new StringBuilder();
            if (Type == SqlLikeType.StartsWith || Type == SqlLikeType.Contains) {
                builder.Append("%");
            }
            builder.Append(Value);
            if (Type == SqlLikeType.EndsWith || Type == SqlLikeType.Contains) {
                builder.Append("%");
            }
            var parameter = list.AddOrGet(builder.ToString());
            return parameter.ToString();
            //var parameter = list.AddOrGet(Value);
            //var builder = new StringBuilder();
            //if (Type == SqlLikeType.StartsWith || Type == SqlLikeType.Contains) {
            //    builder.Append("'%' + ");
            //}
            //builder.Append(parameter);
            //if (Type == SqlLikeType.EndsWith || Type == SqlLikeType.Contains) {
            //    builder.Append(" + '%'");
            //}
            //return builder.ToString();
        }

        public override string ToString() {
            var builder = new StringBuilder();
            builder.Append("'");
            if (Type == SqlLikeType.StartsWith || Type == SqlLikeType.Contains) {
                builder.Append("%");
            }
            builder.Append(Value);
            if (Type == SqlLikeType.EndsWith || Type == SqlLikeType.Contains) {
                builder.Append("%");
            }
            builder.Append("'");
            return builder.ToString();
        }
    }
}
