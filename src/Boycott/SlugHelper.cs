namespace Boycott {
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    public class SlugHelper {
        public static string Generate(string text) {
            text = text.ToLower();
            text = Regex.Replace(text, @"[^\w]+", " ");
            text = Regex.Replace(text, @"\s+", "-");
            text = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            
            for (int ich = 0; ich < text.Length; ich++) {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(text[ich]);
                if (uc != UnicodeCategory.NonSpacingMark) {
                    sb.Append(text[ich]);
                }
            }
            
            return sb.ToString();
        }
    }
}
