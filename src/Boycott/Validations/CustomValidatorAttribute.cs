namespace Boycott.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CustomValidatorAttribute : ValidationAttribute {
        public CustomValidatorAttribute() {
            Validator = new CustomValidator();
        }

        public string ValidateMethod {
            get {
                return ((CustomValidator)Validator).CustomMethodName;
            }
            set {
                ((CustomValidator)Validator).CustomMethodName = value;
            }
        }
    }
}
