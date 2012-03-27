namespace Boycott.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using System.Collections.Specialized;

    public class Validator<T> {
        private Dictionary<string, StringCollection> messages;

        public Validator() {
            messages = new Dictionary<string, StringCollection>();
            Type = typeof(T);
            ErrorMessage = new ErrorMessageStringCollection();
            ExtractProperties();
            ExtractValidators();
        }

        private void ExtractProperties() {
            Properties = new Dictionary<string, PropertyInfo>();
            foreach (var property in Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if (property.CanWrite) Properties.Add(property.Name, property);
            }
        }

        private void ExtractValidators() {
            Validators = new Dictionary<string, IEnumerable<IValidator>>();
            foreach (var property in Properties) {
                var validations = property.Value.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>().ToList();
                Validators.Add(property.Key, validations.ConvertAll(v => v.Validator));
            }
        }

        public Type Type { get; set; }
        public Dictionary<string, PropertyInfo> Properties { get; private set; }
        public Dictionary<string, IEnumerable<IValidator>> Validators { get; private set; }

        public ErrorMessageStringCollection ErrorMessage { get; private set; }
        public bool HasErrors {
            get {
                return ErrorMessage.HasErrors;
            }
        }

        public bool Validate(T instance) {
            var isValid = true;

            foreach (var property in Properties) {
                if (!Validate(instance, property.Key)) {
                    isValid = false;
                }
            }
            return isValid;
        }

        public bool Validate(T instance, string propertyName) {
            if (!Properties.ContainsKey(propertyName)) return true;

            bool isValid = true;

            if (Validators.ContainsKey(propertyName)) {
                ErrorMessage.Clear(propertyName);
                foreach (var validator in Validators[propertyName]) {
                    if (!validator.Validate(instance, Properties[propertyName])) {
                        isValid = false;
                        ErrorMessage.Add(propertyName, validator.Message);
                    }
                }
            }

            return isValid;
        }
    }
}
