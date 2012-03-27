namespace Boycott.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;

    public class ValidatesPresenceOf : ValidatorBase {
        public ValidatesPresenceOf() {
            Message = "{0} can't be blank";
        }

        protected override bool ValidateInternal(object value, object instance) {
            var valid = true;

            if (value == null) {
                valid = false;
            } else if (value.GetType() == typeof(string) && (string)value == string.Empty) {
                valid = false;
            }

            return valid;
        }
    }
}
