namespace Boycott.SqlTranslator {
    public abstract class SqlExpression {
        public virtual string ToParametrizedString(SqlParameterCollection list) {
            return ToString();
        }
    }
}
