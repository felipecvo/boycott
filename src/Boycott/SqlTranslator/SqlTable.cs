namespace Boycott.SqlTranslator {
    public class SqlTable {
        public string Alias { get; set; }
        public string Name { get; set; }

        public override int GetHashCode() {
            return Alias.GetHashCode() ^ Name.GetHashCode();
        }

        public bool Equals(SqlTable obj) {
            return Alias.Equals(obj.Alias) && Name.Equals(obj.Name);
        }

        public override bool Equals(object obj) {
            return Equals((SqlTable)obj);
        }

        public override string ToString() {
            return string.Format("{0} AS {1}", Name, Alias);
        }
    }
}
