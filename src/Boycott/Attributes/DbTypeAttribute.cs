namespace Boycott.Attributes {
    using System;
    using Boycott.Migrate;

    public class DbTypeAttribute : Attribute {
        public DbType DbType { get; set; }
    }
}
