namespace Boycott.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;

    public abstract class ValidatorBase : IValidator {
        private string message;

        protected abstract bool ValidateInternal(object value, object instance);

        public bool Validate(object instance, PropertyInfo property) {
            var value = property.GetValue(instance, null);

            var valid = ValidateInternal(value, instance);

            if (!valid) {
                SetMessage(property.Name);
            } else {
                ClearMessage();
            }

            return valid;
        }

        protected string MessageFormat { get; set; }

        public string Message {
            get {
                return message;
            }
            set {
                MessageFormat = message = value;
            }
        }

        protected void SetMessage(params string[] args) {
            if (string.IsNullOrEmpty(MessageFormat)) return;

            this.message = string.Format(MessageFormat, args);
        }

        protected void ClearMessage() {
            message = null;
        }
    }
}
