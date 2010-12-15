namespace Boycott.Objects {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Boycott.Attributes;

    internal class TableMapper {
        static internal TableMapper GetTableMapper(Type type) {
            var mapper = new TableMapper();
            
            var attributes = type.GetCustomAttributes(typeof(TableAttribute), false);
            if (attributes != null && attributes.Length > 0 && !string.IsNullOrEmpty(((TableAttribute)attributes[0]).Name)) {
                mapper.Name = ((TableAttribute)attributes[0]).Name;
            } else {
                mapper.Name = Configuration.NamingProvider.GetTableName(type.Name);
            }
            
            var properties = type.GetProperties();
            var inheritedProperties = typeof(Base).GetProperties().ToList();
            mapper.Columns = new List<ColumnMapper>();
            mapper.PrimaryKeyColumns = new List<ColumnMapper>();
            
            for (int i = 0; i < properties.Length; i++) {
                var info = properties[i];
                
                if (!info.CanWrite)
                    continue;
                if (inheritedProperties.Find(p => p.Name == info.Name) != null)
                    continue;
                
                var customProperty = info.GetCustomAttributes(typeof(CustomPropertyAttribute), false).FirstOrDefault();
                if (customProperty != null)
                    continue;
                
                var column = new ColumnMapper { PropertyInfo = info };
                
                var attribute = info.GetCustomAttributes(typeof(ColumnAttribute), false);
                column.Name = Configuration.NamingProvider.GetColumnName(info.Name);
                if (attribute != null && attribute.Length > 0) {
                    var columnAttribute = (ColumnAttribute)attribute[0];
                    if (columnAttribute.HasName)
                        column.Name = columnAttribute.Name;
                    column.IsPrimaryKey = columnAttribute.IsPrimaryKey;
                } else {
                    column.IsPrimaryKey = Configuration.NamingProvider.IsPrimaryKey(info.Name, type.Name);
                }
                
                column.ComplexType = ColumnMapper.IsComplexType(info.PropertyType);
                
                mapper.Columns.Add(column);
                if (column.IsPrimaryKey) {
                    mapper.PrimaryKeyColumns.Add(column);
                }
            }
            
            return mapper;
        }

        public string Name { get; set; }
        public List<ColumnMapper> Columns { get; set; }
        public List<ColumnMapper> PrimaryKeyColumns { get; set; }
    }
}
