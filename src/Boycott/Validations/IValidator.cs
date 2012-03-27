namespace Boycott.Validations {
    using System.Reflection;

    public interface IValidator {
        bool Validate(object instance, PropertyInfo property);
        string Message { get; set; }
    }
}
