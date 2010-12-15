namespace Boycott.SqlTranslator {
    public class SqlWhereClause : SqlExpression {
        public SqlExpression Left { get; set; }
        public SqlOperator Operator { get; set; }
        public SqlExpression Right { get; set; }

        public override int GetHashCode() {
            return Left.GetHashCode() + Operator.GetHashCode() + Right.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is SqlWhereClause))
                return false;
            var sql = (SqlWhereClause)obj;
            var @equals = Left.Equals(sql.Left);
            @equals &= Operator.Equals(sql.Operator);
            @equals &= Right.Equals(sql.Right);
            return @equals;
        }

        public override string ToString() {
            return string.Format("({0} {1} {2})", Left, Operator, Right);
        }

        public override string ToParametrizedString(SqlParameterCollection list) {
            return string.Format("({0} {1} {2})", Left.ToParametrizedString(list), Operator, Right.ToParametrizedString(list));
        }
    }
}
