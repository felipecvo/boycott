namespace Boycott.SqlTranslator {
    public class SqlColumnOutput {
        private string text;
        public SqlColumnOutput(string text) {
            this.text = text;
        }
        public override string ToString() {
            return text;
        }
        public override bool Equals(object obj) {
            return this.ToString().Equals(obj.ToString());
        }
        public override int GetHashCode() {
            return ToString().GetHashCode();
        }
    }
}
