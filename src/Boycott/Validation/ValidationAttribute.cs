namespace Boycott.Validation {
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ValidationAttribute : Attribute {
        public string PropertyName { get; set; }
        public string Message { get; set; }
        public object Options { get; set; }

        public abstract void Validate(Base value);
    }
}
