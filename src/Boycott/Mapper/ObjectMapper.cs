namespace Boycott.Mapper {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Boycott.Attributes;
    using Boycott.Extensions;
    using Boycott.Helpers;
    using Boycott.Migrate;

    public class ObjectMapper : AbstractMapper {
        public bool IsSynchronizable { get; set; }
        public List<string> IgnoreColumns { get; set; }

        public ObjectMapper(Type type) {
            var t = Objects.TableMapper.GetTableMapper(type);
            TableName = t.Name;

            var ignoreColumns = type.GetCustomAttributes(typeof(IgnoreColumnsAttribute), true).FirstOrDefault() as IgnoreColumnsAttribute;
            if (ignoreColumns != null) {
                IgnoreColumns = ignoreColumns.Columns;
            }
            IsSynchronizable = type.GetCustomAttributes(typeof(NotSynchronizableAttribute), true).Length == 0;

            Columns = new List<DbColumn>();
            foreach (var col in t.Columns) {
                var dbcolumn = new DbColumn();
                var columnAttribute = col.PropertyInfo.GetAttribute<ColumnAttribute>();
                dbcolumn.Name = columnAttribute == null ? col.Name : columnAttribute.Name;
                dbcolumn.Type = col.PropertyInfo.GetAttribute<DbTypeAttribute>(typeof(DbTypeAttribute), GetDbType).DbType;
                dbcolumn.IsPrimaryKey = col.PropertyInfo.HasAttribute(typeof(PrimaryKeyAttribute)) || col.Name.Equals("id") || (columnAttribute != null && columnAttribute.IsPrimaryKey);
                dbcolumn.Nullable = !col.PropertyInfo.HasAttribute(typeof(NotNullableAttribute));
                if (col.Name.Equals("id"))
                    dbcolumn.Nullable = false;
                if (col.PropertyInfo.HasAttribute(typeof(NumericAttribute))) {
                    var num = col.PropertyInfo.GetAttribute<NumericAttribute>();
                    dbcolumn.Scale = num.Scale;
                    dbcolumn.Precision = num.Precision;
                }
                if (col.PropertyInfo.HasAttribute(typeof(ColumnLimitAttribute))) {
                    dbcolumn.Limit = col.PropertyInfo.GetAttribute<ColumnLimitAttribute>().Limit;
                } else if (dbcolumn.Type == DbType.String) {
                    dbcolumn.Limit = 255;
                }
                dbcolumn.AutoIncrement = col.PropertyInfo.HasAttribute(typeof(AutoIncrementAttribute)) || col.Name.Equals("id");
                if (col.PropertyInfo.HasAttribute(typeof(DefaultValueAttribute))) {
                    dbcolumn.DefaultValue = col.PropertyInfo.GetAttribute<DefaultValueAttribute>().Value;
                }
                dbcolumn.IsSynchronizable = !col.PropertyInfo.HasAttribute(typeof(NotSynchronizableAttribute));
                Columns.Add(dbcolumn);
            }
        }

        private DbTypeAttribute GetDbType(Type type) {
            DbType dbType;
            switch (type.Name) {
                case "String":
                    dbType = DbType.String;
                    break;
                case "Text":
                    dbType = DbType.Text;
                    break;
                case "Int16":
                    dbType = DbType.Short;
                    break;
                case "Int32":
                    dbType = DbType.Integer;
                    break;
                case "Int64":
                    dbType = DbType.Long;
                    break;
                case "Double":
                    dbType = DbType.Float;
                    break;
                case "Decimal":
                    dbType = DbType.Decimal;
                    break;
                case "DateTime":
                    dbType = DbType.DateTime;
                    break;
                //case "timestamp":
                //  dbType = DbType.Timestamp;
                //  break;
                //case "time":
                //  dbType = DbType.Time;
                //  break;
                //case "date":
                //  dbType = DbType.Date;
                //  break;
                case "Boolean":
                    dbType = DbType.Boolean;
                    break;
                case "Short":
                    dbType = DbType.Short;
                    break;
                default:
                    if (type.IsEnum) {
                        dbType = DbType.Short;
                    } else {
                        dbType = DbType.String;
                    }
                    break;
            }
            
            return new DbTypeAttribute { DbType = dbType };
        }

        public static List<Type> GetTypes() {
            var list = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                list.AddRange(assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Base))).ToList());
            }
            list.Remove(typeof(Base<>));
            return list;
        }
    }
}
