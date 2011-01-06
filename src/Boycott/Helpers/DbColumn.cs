namespace Boycott.Helpers {
    using System;
    using System.Text;
    using Boycott.Migrate;

    public class DbColumn {
        public string Name { get; set; }
        public DbType Type { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool Nullable { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public int Limit { get; set; }
        public bool AutoIncrement { get; set; }
        public string DefaultValue { get; set; }
        public bool IsSynchronizable { get; set; }

        public override int GetHashCode() {
            var safeCode = Name.GetHashCode();
            safeCode ^= Type.GetHashCode();
            safeCode ^= IsPrimaryKey.GetHashCode();
            safeCode ^= Nullable.GetHashCode();
            safeCode ^= Precision.GetHashCode();
            safeCode ^= Scale.GetHashCode();
            safeCode ^= Limit.GetHashCode();
            safeCode ^= AutoIncrement.GetHashCode();
            safeCode ^= DefaultValue.GetHashCode();
            return safeCode;
        }

        public override bool Equals(object obj) {
            if (!(obj is DbColumn))
                return false;
            
            return Equals((DbColumn)obj);
        }

        public bool Equals(DbColumn obj) {
            var eql = Name.Equals(obj.Name);
            eql &= Type.Equals(obj.Type);
            eql &= IsPrimaryKey.Equals(obj.IsPrimaryKey);
            eql &= Nullable.Equals(obj.Nullable);
            eql &= Precision.Equals(obj.Precision);
            eql &= Scale.Equals(obj.Scale);
            eql &= Limit.Equals(obj.Limit);
            eql &= AutoIncrement.Equals(obj.AutoIncrement);
            eql &= DefaultValue != null ? DefaultValue.Equals(obj.DefaultValue) : DefaultValue == obj.DefaultValue;
            return eql;
        }

        public override string ToString() {
            var precision = Precision;
            if (Limit > 0)
                precision = Limit;
            
            var extraType = new StringBuilder();
            if (precision > 0) {
                extraType.AppendFormat("({0}", precision);
                if (Scale > 0) {
                    extraType.AppendFormat(",{0}", Scale);
                }
                extraType.Append(")");
            }
            var defaultValue = string.Empty;
            if (!string.IsNullOrEmpty(DefaultValue)) {
                defaultValue = string.Format(" DEFAULT {0}", DefaultValue);
            }
            var nullable = Nullable ? "NULL" : "NOT NULL";
            var autoIncrement = AutoIncrement ? string.Format(" {0}", Configuration.DatabaseProvider.GetAutoIncrement()) : string.Empty;
            
            return string.Format("{0} {1}{2}{3} {4}{5}", Configuration.DatabaseProvider.Quote(Name), Configuration.DatabaseProvider.GetDbType(Type), extraType, defaultValue, nullable, autoIncrement);
        }
    }
}
