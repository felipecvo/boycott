namespace Boycott.Validation {
    using System;
    using System.Collections.Generic;

    public class ValidatesPresenceOfAttribute : ValidationAttribute {
        #region IValidate Members

        public override void Validate(Base value) {
            var property = value.GetType().GetProperty(PropertyName);
            var propertyValue = property.GetValue(value, null);
            var code = Type.GetTypeCode(property.PropertyType);
            switch (code) {
                case TypeCode.DateTime:
                    ValidateDateTime(propertyValue, value.Errors);
                    break;
                default:
                    ValidateDefault(propertyValue, value.Errors);
                    break;
            }
        }

        private string GetErrorText() {
            return string.IsNullOrEmpty(Message) ? "The value of '{0}' can't be empty." : Message;
        }

        private string GetErrorText2() {
            return string.IsNullOrEmpty(Message) ? "The value of '{0}' must be a valid datetime." : Message;
        }

        private void ValidateDefault(object propertyValue, List<string> list) {
            if (propertyValue == null || string.IsNullOrEmpty(propertyValue.ToString())) {
                list.Add(string.Format(GetErrorText(), PropertyName));
            }
        }

        private void ValidateDateTime(object propertyValue, List<string> list) {
            if (((DateTime)propertyValue) == DateTime.MinValue || ((DateTime)propertyValue) == DateTime.MaxValue) {
                list.Add(string.Format(GetErrorText2(), PropertyName));
            }
        }
        
        #endregion
    }
}
