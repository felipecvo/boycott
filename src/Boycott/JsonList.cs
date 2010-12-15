namespace Boycott {
    using System.Collections.Generic;
    using System.Text;

    public class JsonList {
        public static string ToJson<T>(List<T> list) where T : Base<T> {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            var first = true;
            foreach (var item in list) {
                if (!first)
                    sb.AppendLine(",");
                sb.Append(item.ToJson());
                first = false;
            }
            sb.AppendLine();
            sb.AppendLine("]");
            
            return sb.ToString();
        }
    }
}
