namespace Boycott.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.Specialized;

    public class ErrorMessageStringCollection {
        private string baseText;
        private Dictionary<string, List<string>> messages;

        public ErrorMessageStringCollection() {
            messages = new Dictionary<string, List<string>>();
        }

        public ErrorMessageStringCollection(string baseText) : this() {
            this.baseText = baseText;
        }

        public static implicit operator ErrorMessageStringCollection(string errorsMessage) {
            return new ErrorMessageStringCollection(errorsMessage);
        }

        public static explicit operator string(ErrorMessageStringCollection errorsMessage) {
            return errorsMessage.ToString();
        }

        public void Add(string key, string message) {
            if (!messages.ContainsKey(key)) {
                messages[key] = new List<string>();
            }
            messages[key].Add(message);
        }

        public string this[string key] {
            get {
                if (messages.ContainsKey(key)) {
                    return string.Join("\n", messages[key].ToArray());
                } else {
                    return null;
                }
            }
        }

        public override string ToString() {
            var builder = new StringBuilder(baseText);
            bool first = true;

            foreach (var item in messages) {
                if (!first || (first && !string.IsNullOrEmpty(baseText))) builder.Append("\n");
                builder.Append(string.Join("\n", item.Value.ToArray()));
                first = false;
            }

            return builder.ToString();
        }

        public void Clear(string key) {
            messages.Remove(key);
        }

        public bool HasErrors {
            get {
                return messages.Count > 0;
            }
        }
    }
}
