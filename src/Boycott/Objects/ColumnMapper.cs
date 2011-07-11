namespace Boycott.Objects {
    using System;
    using System.Reflection;

    internal class ColumnMapper {
        public ColumnMapper() {
            ComplexType = false;
        }

        public string Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool ComplexType { get; set; }
        public PropertyInfo PropertyInfo { get; set; }

        public static bool IsComplexType(Type type) {
            if (type.IsEnum) return false;

            switch (type.Name) {
                case "SByte":
                case "Byte":
                case "Char":
                case "String":
                case "Int16":
                case "UInt16":
                case "Int32":
                case "UInt32":
                case "Int64":
                case "UInt64":
                case "Single":
                case "Decimal":
                case "Double":
                case "DateTime":
                case "Boolean":
                case "TimeSpan":
                    return false;
                default:
                    return true;
            }
        }
    }
}
