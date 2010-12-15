namespace Boycott.SqlTranslator {
    using System.Collections.Generic;

    public class SqlParameterCollection : List<SqlParameter> {
        public static string DefaultParameterName = "p";

        public SqlParameter AddOrGet(object value) {
            lock (typeof(SqlParameterCollection)) {
                var existing = Find(x => x.Value == value);
                if (existing == null) {
                    var nextParameter = this.Count + 1;
                    var paramName = string.Format("{0}{1}{2}", Configuration.DatabaseProvider.ParameterPrefix, DefaultParameterName, nextParameter);
                    existing = new SqlParameter { Name = paramName, Value = value };
                    Add(existing);
                }
                return existing;
            }
        }
    }
}
