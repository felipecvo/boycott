namespace Boycott.SqlTranslator {
    public class SqlWhereColumn : SqlExpression {
        public string Prefix { get; set; }
        public string Name { get; set; }

        public override int GetHashCode() {
            return Prefix.GetHashCode() + Name.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is SqlWhereColumn))
                return false;
            var sql = (SqlWhereColumn)obj;
            var @equals = Prefix.Equals(sql.Prefix);
            @equals &= Name.Equals(sql.Name);
            return @equals;
        }

        public override string ToString() {
            return string.Format("{0}.{1}", Prefix, Name);
        }
    }
}
