namespace Boycott.SqlTranslator {
    public class SqlJoin : SqlTable {
        public string JoinAlias { get; set; }
        public string JoinTable { get; set; }
        public SqlWhereColumn Left { get; set; }
        public SqlWhereColumn Right { get; set; }

        public override string ToString() {
            return string.Format("INNER JOIN {0} AS {1} ON {2} = {3}", JoinTable, JoinAlias, Left, Right);
        }

        public override int GetHashCode() {
            var code = JoinAlias.GetHashCode();
            code ^= JoinTable.GetHashCode();
            code ^= Left.GetHashCode();
            code ^= Right.GetHashCode();
            return code;
        }

        public override bool Equals(object obj) {
            var obj2 = (SqlJoin)obj;
            var @equals = JoinAlias.Equals(obj2.JoinAlias);
            @equals &= JoinTable.Equals(obj2.JoinTable);
            @equals &= Left.Equals(obj2.Left);
            @equals &= Right.Equals(obj2.Right);
            return @equals;
        }
    }
}
