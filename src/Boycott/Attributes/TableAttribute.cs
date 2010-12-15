namespace Boycott.Attributes {
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute {
        public string Name { get; set; }
    }
}
