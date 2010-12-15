namespace Boycott.SqlTranslator {
    using System;
    using System.Text;

    public class SqlWhereLikeColumn : SqlExpression {
        public SqlWhereColumn Column { get; set; }
        public SqlLikeType Type { get; set; }

        public override int GetHashCode() {
            return Column.GetHashCode() + Type.GetHashCode();
        }

        public override bool Equals(object obj) {
            var sql = (SqlWhereLikeColumn)obj;
            var @equals = Column.Equals(sql.Column);
            @equals &= Type.Equals(sql.Type);
            return @equals;
        }

        public override string ToString() {
            var builder = new StringBuilder();
            if (Type == SqlLikeType.StartsWith || Type == SqlLikeType.Contains) {
                builder.Append("'%' + ");
            }
            builder.Append(Column);
            if (Type == SqlLikeType.EndsWith || Type == SqlLikeType.Contains) {
                builder.Append(" + '%'");
            }
            return builder.ToString();
        }
    }
}
