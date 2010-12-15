namespace Boycott.Attributes {
    using System;

    public class NumericAttribute : Attribute {
        public int Precision { get; set; }
        public int Scale { get; set; }
    }
}
