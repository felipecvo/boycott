namespace Boycott.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;

    public class CustomValidator : ValidatorBase {
        public delegate bool CustomValidateMethod(object value);

        public CustomValidateMethod CustomMethod { get; set; }

        public string CustomMethodName { get; set; }

        protected override bool ValidateInternal(object value, object instance) {
            if (CustomMethod == null) {
                return (bool)instance.GetType().GetMethod(CustomMethodName).Invoke(instance, new object[] { value });
            }
            return CustomMethod.Invoke(value);
        }
    }
}
