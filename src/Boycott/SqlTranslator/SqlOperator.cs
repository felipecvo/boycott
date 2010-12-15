namespace Boycott.SqlTranslator {
    public class SqlOperator {
        private SqlOperator(string oper) {
            Operator = oper;
        }

        public string Operator { get; private set; }

        public override string ToString() {
            return Operator;
        }

        private static SqlOperator @add;
        public static SqlOperator Add {
            get { return @add ?? (@add = new SqlOperator("+")); }
        }
        private static SqlOperator subtract;
        public static SqlOperator Subtract {
            get { return subtract ?? (subtract = new SqlOperator("-")); }
        }
        private static SqlOperator multiply;
        public static SqlOperator Multiply {
            get { return multiply ?? (multiply = new SqlOperator("*")); }
        }
        private static SqlOperator divide;
        public static SqlOperator Divide {
            get { return divide ?? (divide = new SqlOperator("/")); }
        }
        private static SqlOperator modulo;
        public static SqlOperator Modulo {
            get { return modulo ?? (modulo = new SqlOperator("%")); }
        }
        private static SqlOperator and;
        public static SqlOperator And {
            get { return and ?? (and = new SqlOperator("AND")); }
        }
        private static SqlOperator or;
        public static SqlOperator Or {
            get { return or ?? (or = new SqlOperator("OR")); }
        }
        private static SqlOperator lessThan;
        public static SqlOperator LessThan {
            get { return lessThan ?? (lessThan = new SqlOperator("<")); }
        }
        private static SqlOperator lessThanOrEqual;
        public static SqlOperator LessThanOrEqual {
            get { return lessThanOrEqual ?? (lessThanOrEqual = new SqlOperator("<=")); }
        }
        private static SqlOperator greaterThan;
        public static SqlOperator GreaterThan {
            get { return greaterThan ?? (greaterThan = new SqlOperator(">")); }
        }
        private static SqlOperator greaterThanOrEqual;
        public static SqlOperator GreaterThanOrEqual {
            get { return greaterThanOrEqual ?? (greaterThanOrEqual = new SqlOperator(">=")); }
        }
        private static SqlOperator equal;
        public static SqlOperator Equal {
            get { return equal ?? (equal = new SqlOperator("=")); }
        }
        private static SqlOperator notEqual;
        public static SqlOperator NotEqual {
            get { return notEqual ?? (notEqual = new SqlOperator("<>")); }
        }
        private static SqlOperator like;
        public static SqlOperator Like {
            get { return like ?? (like = new SqlOperator("like")); }
        }
        private static SqlOperator exclusiveOr;
        public static SqlOperator ExclusiveOr {
            get { return exclusiveOr ?? (exclusiveOr = new SqlOperator("|")); }
        }
        private static SqlOperator _in;
        public static SqlOperator In {
            get { return _in ?? (_in = new SqlOperator("IN")); }
        }
    }
}
