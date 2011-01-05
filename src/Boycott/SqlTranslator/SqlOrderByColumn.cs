namespace Boycott.SqlTranslator {
    public class SqlOrderByColumn : SqlExpression {
        public string Prefix { get; set; }
        public string Name { get; set; }
        public SqlOrderBy Direction { get; set; }

        public override string ToString() {
            return string.Format("{0}.{1} {2}", Prefix, Name, Direction);
        }

        public override int GetHashCode() {
            var code = Prefix.GetHashCode();
            code ^= Name.GetHashCode();
            code ^= Direction.GetHashCode();
            return code;
        }
        public override bool Equals(object obj) {
            var obj1 = (SqlOrderByColumn)obj;
            var @equals = Prefix.Equals(obj1.Prefix);
            @equals &= Name.Equals(obj1.Name);
            @equals &= Direction.Equals(obj1.Direction);
            return @equals;
        }
    }
}
