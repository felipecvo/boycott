namespace Boycott.SqlTranslator {
    public class SqlParameter : SqlExpression {
        public string Name { get; set; }
        public object Value { get; set; }

        public override string ToString() {
            return Name;
        }
    }
}
