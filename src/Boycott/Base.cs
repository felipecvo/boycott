namespace Boycott {
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Boycott.Objects;
    using Boycott.Provider;
    using System;
    using Boycott.Validation;

    public abstract class Base {
        static Base() {
            Provider = Configuration.DatabaseProvider;
            Caption = "<p>Please, correct the following errors:</p>";
        }

        public Base() {
            IsNew = true;
        }

        public static DatabaseProvider Provider { get; private set; }

        protected void AddError(string message) {
            if (Errors == null)
                Errors = new List<string>();
            Errors.Add(message);
        }

        internal List<string> Errors { get; set; }

        internal bool HasErrorsInternal {
            get { return Errors != null && Errors.Count > 0; }
        }

        public static string Caption { get; set; }

        protected virtual string GetErrorHtml() {
            var template = "<div class=\"validation-error\">" + Caption + "<ul>{0}</ul></div>";

            return string.Format(template, GetItemErrorHtml());
        }

        protected virtual string GetItemErrorHtml() {
            var template = "<li>{0}</li>";
            var builder = new StringBuilder();
            foreach (var item in Errors) {
                builder.Append(string.Format(template, item));
            }
            return builder.ToString();
        }

        [Browsable(false)]
        public string ValidationSummary {
            get {
                if (HasErrorsInternal)
                    return GetErrorHtml();
                return string.Empty;
            }
        }

        internal abstract TableMapper Mapper { get; }

        [Browsable(false)]
        public bool IsNew { get; internal set; }

        [Browsable(false)]
        public bool ReadOnly { get; protected set; }

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

        public bool Destroy() {
            if (Mapper.PrimaryKeyColumns.Count != 1)
                throw new Exception("Only support one primary key column");

            try {
                var pk = Mapper.PrimaryKeyColumns.First();
                Configuration.DatabaseProvider.ExecuteDelete((int)pk.PropertyInfo.GetValue(this, null), Mapper.Name, pk.Name);
            } catch {
                return false;
            }

            return ReadOnly = true;
        }

        #endregion

    }
}
