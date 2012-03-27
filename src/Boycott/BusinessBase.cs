namespace Boycott {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Boycott.Objects;
    using System.Linq.Expressions;
    using Boycott.Validations;
    using Boycott.Core;
    using System.ComponentModel;

    public class BusinessBase<T> : Base, IDataErrorInfo where T: class {
        static TableMapper mapper;

        public BusinessBase() {
            Validator = new Validator<T>();
        }

        static BusinessBase() {
            var type = typeof(T);

            mapper = TableMapper.GetTableMapper(type);

            var attributes = type.GetCustomAttributes(typeof(Boycott.Validation.ValidationAttribute), true);
            ValidationsAttributes = new List<Boycott.Validation.ValidationAttribute>();
            foreach (Boycott.Validation.ValidationAttribute attribute in attributes) {
                ValidationsAttributes.Add(attribute);
            }
        }

        private Validator<T> Validator { get; set; }

        internal override TableMapper Mapper {
            get { return mapper; }
        }

        public static DbQueryable<T> db {
            get { return new DbQueryable<T>(); }
        }

        public static List<T> All() {
            return Finder.All<T>();
        }

        public static bool Delete(int id) {
            if (mapper.PrimaryKeyColumns.Count > 1)
                throw new Exception("Only support one primary key column");
            try {
                Configuration.DatabaseProvider.ExecuteDelete(id, mapper.Name, mapper.PrimaryKeyColumns.First().Name);
            } catch {
                return false;
            }

            return true;
        }

        public bool HasErrors {
            get {
                return Validator.HasErrors;
            }
        }

        public bool Validate() {
            return Validator.Validate(this as T);
        }

        string IDataErrorInfo.Error {
            get {
                if (Validator.HasErrors) {
                    return Validator.ErrorMessage.ToString();
                } else {
                    return string.Empty;
                }
            }
        }

        string IDataErrorInfo.this[string columnName] {
            get {
                if (Validator.Validate(this as T, columnName)) {
                    return string.Empty;
                } else {
                    return Validator.ErrorMessage[columnName];
                }
            }
        }
    }
}
