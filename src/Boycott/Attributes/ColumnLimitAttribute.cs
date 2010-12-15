namespace Boycott.Attributes {
    using System;

    public class ColumnLimitAttribute : Attribute {
        public int Limit { get; set; }
    }
}
