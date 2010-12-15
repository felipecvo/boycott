namespace Boycott.Attributes {
    using System;

    public class ColumnAttribute : Attribute {
        public ColumnAttribute() {
            IsPrimaryKey = false;
        }

        public string Name { get; set; }
        internal bool HasName {
            get { return !string.IsNullOrEmpty(Name); }
        }
        public bool IsPrimaryKey { get; set; }
    }
}
