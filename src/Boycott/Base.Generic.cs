namespace Boycott {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Boycott.Objects;
    using Boycott.Provider;
    using Boycott.Validation;

    public class Base<T> : Base, IQueryable<T>, IOrderedQueryable<T> {
        #region Fields

        private DatabaseProvider provider;
        private Expression expression;
        static TableMapper mapper;

        #endregion

        #region Constructors

        static Base() {
            var type = typeof(T);

            mapper = TableMapper.GetTableMapper(type);
            
            var attributes = type.GetCustomAttributes(typeof(ValidationAttribute), true);
            ValidationsAttributes = new List<ValidationAttribute>();
            foreach (ValidationAttribute attribute in attributes) {
                ValidationsAttributes.Add(attribute);
            }
        }

        public Base() {
            this.provider = Configuration.DatabaseProvider;
            this.expression = Expression.Constant(this);
        }

        //public ActiveRecordBase(DatabaseProvider provider, Expression expression) {
        //    this.provider = provider;
        //    this.expression = expression;
        //}

        #endregion

        #region Static

        private static T _db;
        public static T db {
            get {
                if (_db == null) {
                    _db = (T)Activator.CreateInstance(typeof(T));
                }
                return _db;
            }
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return this.provider.Execute<IEnumerable<T>>(this.expression).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return this.provider.Execute<IEnumerable<T>>(this.expression).GetEnumerator();
        }

        #endregion

        #region IQueryable Members

        Type IQueryable.ElementType {
            get { return GetType(); }
        }

        Expression IQueryable.Expression {
            get { return this.expression; }
        }

        IQueryProvider IQueryable.Provider {
            get { return this.provider; }
        }

        #endregion

        public static List<T> All() {
            return Finder.All<T>();
        }

        public static List<T> FindAll(object conditions) {
            return Finder.FindAll<T>(conditions);
        }

        public static List<T> FindBy(string field, object value)
        {
            ColumnMapper column = mapper.Columns.Find(x => x.Name == field.ToLower());
            return (column != null? Finder.FindBy<T>(value, column.PropertyInfo) : new List<T>());
        }

        public static T Find(int id) {
            return Finder.Find<T>(id, mapper.PrimaryKeyColumns.First().PropertyInfo);
            //var tipo = Expression.Constant(Activator.CreateInstance<T>());
            //var parameter = (ParameterExpression)ParameterExpression.Parameter(tipo.Type, "n");
            
            //var x = Expression.Property(parameter, mapper.PrimaryKeyColumns.First().PropertyInfo);
            
            //var unary = UnaryExpression.Lambda(BinaryExpression.Equal(x, Expression.Constant(id)), new ParameterExpression[] { parameter });
            
            //var whereMethod = typeof(Queryable).GetMethods()[0].MakeGenericMethod(new Type[] { typeof(T) });
            
            //var method = typeof(Queryable).GetMethods()[1].MakeGenericMethod(new Type[] { typeof(T) });
            
            //var m2 = Expression.Call(null, whereMethod, new Expression[] { tipo, unary });
            //var m = Expression.Call(null, method, new Expression[] { m2 });
            
            //return Configuration.DatabaseProvider.Execute<T>(m);
        }

        protected static List<ValidationAttribute> ValidationsAttributes { get; set; }

        private void Validate() {
            Errors = new List<string>();
            foreach (var item in ValidationsAttributes) {
                item.Validate(this);
            }
            if (Errors.Count > 0)
                throw new ValidationException();
        }

        #region CUD Methods

        public delegate bool CallbackHandler(object sender);

        public event CallbackHandler BeforeSave;
        public event CallbackHandler BeforeCreate;
        public event CallbackHandler BeforeUpdate;

        public bool Save() {
            if (BeforeSave != null) {
                if (!BeforeSave(this))
                    return false;
            }
            return CreateOrUpdate();
        }

        private bool CreateOrUpdate() {
            if (ReadOnly)
                throw new Exception("ReadOnly");
            var success = IsNew ? Create() : Update();
            if (success)
                ClearCache(Mapper.Name);
            return success;
        }

        private bool Create() {
            try {
                if (BeforeCreate != null) {
                    if (!BeforeCreate(this))
                        return false;
                }
                Validate();
                
                var createdAt = Mapper.Columns.Find(name => string.Compare(name.Name, "created_at") == 0);
                if (createdAt != null)
                    createdAt.PropertyInfo.SetValue(this, DateTime.Now, null);
                var updatedAt = Mapper.Columns.Find(name => string.Compare(name.Name, "updated_at") == 0);
                if (updatedAt != null)
                    updatedAt.PropertyInfo.SetValue(this, DateTime.Now, null);
                
                Provider.ExecuteInsert(this);
                
                return true;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                if (!(ex is ValidationException))
                    this.Errors.Add(ex.Message);
                return false;
            }
        }

        private bool Update() {
            try {
                if (BeforeUpdate != null) {
                    if (!BeforeUpdate(this))
                        return false;
                }
                Validate();
                
                var updatedAt = Mapper.Columns.Find(name => string.Compare(name.Name, "updated_at") == 0);
                if (updatedAt != null)
                    updatedAt.PropertyInfo.SetValue(this, DateTime.Now, null);
                
                Configuration.DatabaseProvider.ExecuteUpdate(this);
                
                return true;
            } catch (Exception ex) {
                if (!(ex is ValidationException))
                    this.Errors.Add(ex.Message);
                return false;
            }
        }

        public static bool Delete(int id) {
            if (mapper.PrimaryKeyColumns.Count > 1)
                throw new Exception("Only support one primary key column");
            try {
                Configuration.DatabaseProvider.ExecuteDelete(id, mapper.Name, mapper.PrimaryKeyColumns.First().Name);
                ClearCache(mapper.Name);
            } catch {
                return false;
            }
            
            return true;
        }

        #endregion

        #region Cache

        private static void ClearCache(string table) {
            if (System.Web.HttpContext.Current != null) {
                foreach (DictionaryEntry i in System.Web.HttpContext.Current.Cache) {
                    if (i.Key.ToString().Contains(table)) {
                        System.Web.HttpContext.Current.Cache.Remove(i.Key.ToString());
                    }
                }
            }
        }

        #endregion

        protected internal override void SetExpression(Expression expression) {
            this.expression = expression;
        }

        internal override TableMapper Mapper {
            get { return mapper; }
        }

        public override string ToString() {
            if (expression is ConstantExpression) {
                return base.ToString();
            }
            return Configuration.DatabaseProvider.GetQueryText(expression);
            ;
        }

        public virtual string ToJson() {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var first = true;
            foreach (var item in mapper.Columns) {
                if (!first)
                    builder.AppendLine(",");
                builder.AppendFormat("\"{0}\"", item.PropertyInfo.Name);
                builder.Append(":");
                if (item.PropertyInfo.PropertyType == typeof(DateTime)) {
                    builder.AppendFormat("\"{0:yyyy-MM-ddTHH:mm:sszzzz}\"", (DateTime)item.PropertyInfo.GetValue(this, null));
                } else {
                    builder.Append(serializer.Serialize(item.PropertyInfo.GetValue(this, null)));
                }
                first = false;
            }
            
            builder.AppendLine();
            builder.AppendLine("}");
            
            return builder.ToString();
        }

        #region Helpers

        public static T Build(NameValueCollection form) {
            var type = typeof(T);
            T obj = (T)Activator.CreateInstance(type);
            return Load(obj, form);
        }

        public static T Load(T obj, NameValueCollection form) {
            var type = typeof(T);
            var properties = type.GetProperties();
            var inheritedProperties = typeof(Base).GetProperties().ToList();
            for (int i = 0; i < properties.Length; i++) {
                var info = properties[i];
                
                if (!info.CanWrite)
                    continue;
                if (inheritedProperties.Find(p => string.Compare(p.Name, info.Name, true) == 0) != null)
                    continue;
                
                var value = form[info.Name];
                if (!string.IsNullOrEmpty(value)) {
                    try {
                        info.SetValue(obj, GetValue(value, info.PropertyType), null);
                    } catch {
                    }
                }
            }
            
            return obj;
        }

        private static object GetValue(string value, Type type) {
            var code = Type.GetTypeCode(type);
            switch (code) {
                case TypeCode.Boolean:
                    value = value.Split(',')[0];
                    return Convert.ToBoolean(value);
                case TypeCode.Byte:
                    return Convert.ToByte(value);
                case TypeCode.Char:
                    return Convert.ToChar(value);
                case TypeCode.DateTime:
                    return Convert.ToDateTime(value);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(value);
                case TypeCode.Double:
                    return Convert.ToDouble(value);
                case TypeCode.Int16:
                    return Convert.ToInt16(value);
                case TypeCode.Int32:
                    return Convert.ToInt32(value);
                case TypeCode.Int64:
                    return Convert.ToInt64(value);
                case TypeCode.SByte:
                    return Convert.ToSByte(value);
                case TypeCode.Single:
                    return Convert.ToSingle(value);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(value);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(value);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value);
                default:
                    if (type.Name == "TimeSpan") {
                        return TimeSpan.Parse(value);
                    } else if (type.IsEnum) {
                        return Enum.Parse(type, value);
                    }
                    return value;
            }
        }
        
        #endregion
    }
}
