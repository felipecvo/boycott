namespace Boycott {
    using System;
    using System.Collections.Generic;

    public class IgnoreColumnsAttribute : Attribute {
        public IgnoreColumnsAttribute(params string[] columnName) {
            Columns = new List<string>(columnName);
        }

        public List<string> Columns { get; private set; }
    }
}

