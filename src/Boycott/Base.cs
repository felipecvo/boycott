namespace Boycott {
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Text;
    using Boycott.Objects;
    using Boycott.Provider;

    public abstract class Base {
        static Base() {
            Provider = Configuration.DatabaseProvider;
            Caption = "<p>Please, correct the following errors:</p>";
        }

        public Base() {
            IsNew = true;
        }

        public static DatabaseProvider Provider { get; private set; }

        protected void AddError(string message) {
            if (Errors == null)
                Errors = new List<string>();
            Errors.Add(message);
        }

        internal List<string> Errors { get; set; }

        internal bool HasErrors {
            get { return Errors != null && Errors.Count > 0; }
        }

        public static string Caption { get; set; }

        protected virtual string GetErrorHtml() {
            var template = "<div class=\"validation-error\">" + Caption + "<ul>{0}</ul></div>";
            
            return string.Format(template, GetItemErrorHtml());
        }

        protected virtual string GetItemErrorHtml() {
            var template = "<li>{0}</li>";
            var builder = new StringBuilder();
            foreach (var item in Errors) {
                builder.Append(string.Format(template, item));
            }
            return builder.ToString();
        }

        [Browsable(false)]
        public string ValidationSummary {
            get {
                if (HasErrors)
                    return GetErrorHtml();
                return string.Empty;
            }
        }

        internal abstract TableMapper Mapper { get; }

        [Browsable(false)]
        public bool IsNew { get; set; }

        [Browsable(false)]
        public bool ReadOnly { get; set; }

        protected internal abstract void SetExpression(Expression expression);
    }
}
