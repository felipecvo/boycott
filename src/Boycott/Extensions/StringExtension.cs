namespace Boycott.Extensions {
    public static class StringExtension {
        public static string Last(this string str) {
            return str.Substring(str.Length - 1);
        }

        public static string Strip(this string str, int length) {
            return str.Substring(0, str.Length - length);
        }

        public static string Singularize(this string word) {
            if (word.EndsWith("ies")) {
                return word.Strip(3) + "y";
            } else if (word.EndsWith("es")) {
                return word.Strip(2) + "s";
            } else {
                return word.Strip(1);
            }
        }

        public static string Pluralize(this string word) {
            if (word.Last() == "s" || word.Last() == "x") {
                return word + "es";
            } else if (word.Last() == "y") {
                return word.Strip(1) + "ies";
            }
            return word + "s";
        }
        
    }
}
