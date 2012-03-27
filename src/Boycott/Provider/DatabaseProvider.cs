namespace Boycott.Provider {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.Script.Serialization;
    using Boycott.Extensions;
    using Boycott.Helpers;
    using Boycott.Logging;
    using Boycott.Objects;
    using Boycott.SqlTranslator;
    using System.Configuration;
    using Boycott.Core;

    public abstract class DatabaseProvider : IQueryProvider {
        #region Fields

        public CultureInfo DefaultCultureInfo = new CultureInfo("en-US");
        private static MethodInfo ExecuteReaderInternalMethodInfo;

        #endregion

        #region Constructor

        static DatabaseProvider() {
            ExecuteReaderInternalMethodInfo = typeof(DatabaseProvider).GetMethod("ExecuteReaderInternal", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        #endregion

        #region Properties

        public string ParameterPrefix { get; protected set; }

        public abstract bool SupportsParameter { get; }

        [Obsolete]
        public bool SupportsMigrations {
            get { throw new InvalidOperationException(); }
        }

        public abstract List<string> Tables { get; }

        public string Database { get; set; }

        #endregion

        #region Public Methods

        public abstract bool DatabaseExists(string databaseName);

        public abstract void CreateDatabase();

        public abstract void DropDatabase();

        public abstract List<DbColumn> GetColumns(string tableName);

        public T Execute<T>(Expression expression) {
            var type = typeof(T);
            var code = Type.GetTypeCode(type);
            var aggregate = false;
            
            switch (code) {
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    return default(T);
                case TypeCode.Object:
                    aggregate = false;
                    break;
                default:
                    aggregate = true;
                    break;
            }
            
            SqlQuery query = QueryTranslator.Translate(expression);
            
            var cached = GetObjectFromCache<T>(query);
            var NULL = default(T);
            if (cached != null && !cached.Equals(NULL))
                return cached;
            
            if (type.Name == "IEnumerable`1") {
                try {
                    var elementType = TypeSystem.GetElementType(type);
                    var mi = ExecuteReaderInternalMethodInfo.MakeGenericMethod(elementType);
                    cached = (T)mi.Invoke(this, new object[] { query });
                } catch (TargetInvocationException ex) {
                    throw ex.InnerException;
                }
            } else if (!aggregate) {
                cached = ExecuteSingleReader<T>(query);
            } else {
                cached = ExecuteScalarInternal<T>(query);
            }
            
            AddObjectToCache(query, cached);
            return cached;
        }

        private string GetCacheKey(SqlQuery query) {
            var values = new List<string>();
            foreach (var parameter in query.Parameters) {
                values.Add(string.Format("{0}={1}", parameter.Name, parameter.Value));
            }
            var key = string.Format("{0}-{1},{2},{3}", query, query.Skip, query.Take, string.Join(",", values.ToArray()));
            return key;
        }

        private void AddObjectToCache(SqlQuery query, object cached) {
            if (cached == null)
                return;
            
            var key = GetCacheKey(query);
            if (System.Web.HttpContext.Current != null) {
                System.Web.HttpContext.Current.Cache.Add(key, cached, null, DateTime.Now.Add(TimeSpan.FromMinutes(1)), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Normal, null);
            }
        }

        private T GetObjectFromCache<T>(SqlQuery query) {
            var key = GetCacheKey(query);
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Cache != null) {
                var cached = System.Web.HttpContext.Current.Cache.Get(key);
                if (cached != null) {
                    return (T)cached;
                }
            }
            return default(T);
        }

        private object ExecuteReaderInternal<T>(SqlQuery query) {
            var list = new List<T>();
            
            if (Logger != null) {
                Logger.Log(GetQueryText(query));
            }
            
            using (var reader = ExecuteReader(query)) {
                while (reader.Read()) {
                    list.Add(LoadFromDataReader<T>(reader));
                }
            }
            return list;
        }

        private T ExecuteScalarInternal<T>(SqlQuery query) {
            var scalar = ExecuteScalar(query);
            try {
                return (T)scalar;
            } catch (InvalidCastException) {
                var convertible = scalar as IConvertible;
                if (convertible != null) {
                    return (T)convertible.ToType(typeof(T), null);
                }
                throw;
            }
        }

        private T ExecuteSingleReader<T>(SqlQuery query) {
            if (Logger != null) {
                Logger.Log(GetQueryText(query));
            }
            
            using (var reader = ExecuteReader(query)) {
                if (reader.Read()) {
                    return LoadFromDataReader<T>(reader);
                }
            }
            
            return default(T);
        }

        private JavaScriptSerializer serializer;
        protected JavaScriptSerializer Serializer {
            get { return serializer ?? (serializer = new JavaScriptSerializer()); }
        }

        private T LoadFromDataReader<T>(IDataReader reader) {
            var result = (T)Activator.CreateInstance(typeof(T));
            var activeRecord = result as Base;
            if (activeRecord != null) {
                for (int i = 0; i < reader.FieldCount; i++) {
                    var property = activeRecord.Mapper.Columns.Find(x => string.Compare(x.Name, reader.GetName(i), true) == 0);
                    if (property != null) {
                        var val = reader[i];
                        if (val != DBNull.Value) {
                            if (property.ComplexType) {
                                try {
                                    if (property.PropertyInfo.PropertyType == typeof(Guid)) {
                                        property.PropertyInfo.SetValue(result, new Guid(reader[i] as string), null);
                                    } else {
                                        var v = Serializer.DeserializeObject(reader.GetString(i));
                                        property.PropertyInfo.SetValue(result, v, null);
                                    }
                                } catch (Exception ex) {
                                    Console.WriteLine(ex.Message);
                                }
                            } else {
                                if (property.PropertyInfo.PropertyType.Name == "Boolean") {
                                    property.PropertyInfo.SetValue(result, Convert.ToBoolean(reader[i]), null);
                                } else {
                                    property.PropertyInfo.SetValue(result, reader[i], null);
                                }
                            }
                        } else {
                            property.PropertyInfo.SetValue(result, null, null);
                        }
                    } else if (reader.GetName(i) == string.Format("{0}_id", activeRecord.Mapper.Name.Singularize())) {
                        property = activeRecord.Mapper.Columns.Find(x => string.Compare(x.Name, "id", true) == 0);
                        if (property != null) {
                            property.PropertyInfo.SetValue(result, reader[i], null);
                        }
                    }
                }
                activeRecord.IsNew = false;
            } else {
                throw new InvalidOperationException("Object is not an ActiveRecordBase descendent");
            }
            return result;
        }

        protected abstract string GetQueryText(SqlQuery query);

        public string GetQueryText(Expression expression) {
            return GetQueryText(QueryTranslator.Translate(expression));
        }

        public abstract IDbConnection GetConnection();

        protected abstract object ExecuteScalar(SqlQuery query);

        public int ExecuteNonQuery(string query) {
            var conn = GetConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = Escape(query);
            
            if (Logger != null) {
                Logger.Log(query);
            }
            
            return cmd.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader(string query) {
            var conn = GetConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            
            if (Logger != null) {
                Logger.Log(query);
            }
            
            return cmd.ExecuteReader();
        }

        protected abstract IDataReader ExecuteReader(SqlQuery query);

        public IQueryable<T> CreateQuery<T>(Expression expression) {
            if (typeof(T).GetInterface("Boycott.Core.ISelfQuery") != null) {
                var query = Activator.CreateInstance(typeof(T));
                ((ISelfQuery)query).SetExpression(expression);
                return query as IQueryable<T>;
            } else {
                Type elementType = expression.Type.GetGenericArguments()[0];
                var query = (DbQueryable<T>)Activator.CreateInstance(typeof(DbQueryable<>).MakeGenericType(elementType));
                query.SetExpression(expression);
                return query;
            }
        }

        #endregion

        #region Internal Methods

        internal string GetDBValue(object value) {
            if (value == null)
                return "NULL";
            
            var code = Type.GetTypeCode(value.GetType());
            switch (code) {
                case TypeCode.DateTime:
                    return string.Format("'{0:yyyy-MM-dd HH:mm:ss}'", value);
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return value.ToString();
                case TypeCode.Boolean:
                    return (bool)value ? "1" : "0";
                case TypeCode.DBNull:
                    return "NULL";
                case TypeCode.Decimal:
                    return ((decimal)value).ToString(DefaultCultureInfo);
                case TypeCode.Double:
                    return ((double)value).ToString(DefaultCultureInfo);
                case TypeCode.Single:
                    return ((float)value).ToString(DefaultCultureInfo);
                default:
                    return string.Format("'{0}'", value);
            }
        }

        #endregion

        #region IQueryProvider Members

        IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression) {
            return CreateQuery<T>(expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression) {
            return CreateQuery<object>(expression);
        }

        T IQueryProvider.Execute<T>(Expression expression) {
            return this.Execute<T>(expression);
        }

        object IQueryProvider.Execute(Expression expression) {
            return this.Execute<object>(expression);
        }

        #endregion

        public static FileLogger Logger { get; set; }

        internal string QuoteTableName(string table) {
            return table;
        }

        internal void ExecuteInsert(Base activeRecordBase) {
            var quoted_attributes = AttributesWithQuotes(false, activeRecordBase);
            var cols = string.Join(", ", quoted_attributes.Keys.ToArray());
            var vals = string.Join(", ", quoted_attributes.Values.ToArray());
            var sql = string.Format("INSERT INTO {0} ({1}) VALUES({2});SELECT @@IDENTITY AS 'LastID';", QuoteTableName(activeRecordBase.Mapper.Name), cols, vals);

            using (var reader = ExecuteReader(sql)) {
                if (reader.Read()) {
                    var prop = activeRecordBase.Mapper.Columns.Find(x => string.Compare(x.Name, "id", true) == 0);
                    if (prop != null) {
                        prop.PropertyInfo.SetValue(activeRecordBase, Convert.ToInt32(reader[0]), null);
                    }
                }
            }
            
            activeRecordBase.IsNew = false;
        }

        internal void ExecuteUpdate(Base activeRecordBase) {
            var quotedAttributes = AttributesWithQuotes(true, activeRecordBase);
            var cols = new List<string>();
            var @where = new List<string>();
            foreach (var attr in quotedAttributes) {
                if (activeRecordBase.Mapper.PrimaryKeyColumns.Find(x => Quote(x.Name) == attr.Key) != null) {
                    @where.Add(string.Format("{0} = {1}", attr.Key, attr.Value));
                } else {
                    cols.Add(string.Format("{0} = {1}", attr.Key, attr.Value));
                }
            }
            var sql = string.Format("UPDATE {0} SET {1} WHERE {2}", activeRecordBase.Mapper.Name, string.Join(",", cols.ToArray()), string.Join("AND ", @where.ToArray()));
            
            ExecuteNonQuery(sql);
        }

        private Dictionary<string, string> AttributesWithQuotes(bool includePrimaryKey, Base record) {
            var quoted = new Dictionary<string, string>();
            foreach (var column in record.Mapper.Columns) {
                if (includePrimaryKey || !column.IsPrimaryKey) {
                    var value = FormatValue(column.PropertyInfo.GetValue(record, null));
                    
                    quoted.Add(Quote(column.Name), value);
                }
            }
            return quoted;
        }

        internal virtual string Quote(string value) {
            return value;
        }

        private static CultureInfo DBCulture = new CultureInfo("en-US");

        private string FormatValue(object value) {
            if (value == null) {
                return "NULL";
            } else if (value is DateTime) {
                return string.Format("'{0}'", ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"));
            } else if (value is Boolean) {
                return (bool)value ? "1" : "0";
            } else if (value is short || value is int || value is long) {
                return value.ToString();
            } else if (value is decimal || value is float || value is double) {
                var iconv = value as IConvertible;
                return iconv.ToString(DBCulture);
            } else {
                if (ColumnMapper.IsComplexType(value.GetType())) {
                    value = Serializer.Serialize(value);
                } else if (value.GetType().IsEnum) {
                    return Convert.ChangeType(value, ((Enum)value).GetTypeCode()).ToString();
                }
                return string.Format("'{0}'", value.ToString().Replace("'", "''"));
            }
        }

        internal void ExecuteDelete(int id, string table, string column) {
            var sql = string.Format("DELETE FROM {0} WHERE {1} = {2};", table, column, id);
            
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// Build a new provider from connectionstring settings
        /// </summary>
        /// <param name="name">connection string key</param>
        /// <returns>new DatabaseProvider based on providerName settings</returns>
        public static DatabaseProvider BuildFromConfig(string name) {
            var connectionSettings = ConfigurationManager.ConnectionStrings[name];

            var type = Assembly.GetAssembly(typeof(DatabaseProvider)).GetType(connectionSettings.ProviderName);

            return Activator.CreateInstance(type, connectionSettings.ConnectionString) as DatabaseProvider;
        }

        public abstract string GetAutoIncrement();

        public abstract string GetDbType(Boycott.Migrate.DbType Type);

        public abstract void Recycle();

        protected abstract string Escape(string query);
    }
}
