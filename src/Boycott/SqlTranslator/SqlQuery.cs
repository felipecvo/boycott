namespace Boycott.SqlTranslator {
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public class SqlQuery {
        public SqlQuery() {
            Tables = new List<SqlTable>();
            ColumnsOutput = new List<SqlColumnOutput>();
            ColumnsOutput.Add(new SqlColumnOutput("*"));
            Joins = new List<SqlJoin>();
            OrderBy = new List<SqlOrderByColumn>();
            LockColumnOutput = false;
            Parameters = new SqlParameterCollection();
        }

        public int Skip { get; set; }
        public int Take { get; set; }
        public SqlExpression WhereClause { get; set; }
        public List<SqlTable> Tables { get; set; }
        private List<SqlColumnOutput> ColumnsOutput { get; set; }
        public List<SqlJoin> Joins { get; set; }
        public List<SqlOrderByColumn> OrderBy { get; set; }
        public bool LockColumnOutput { get; set; }
        public SqlParameterCollection Parameters { get; private set; }

        public void AddColumnOutput(string column) {
            if (LockColumnOutput)
                return;
            
            if (ColumnsOutput.Count == 1 && ColumnsOutput[0].ToString() == "*")
                ColumnsOutput.Clear();
            
            ColumnsOutput.Add(new SqlColumnOutput(column));
        }

        public override string ToString() {
            var builder = new StringBuilder();
            
            builder.Append("SELECT ");
            foreach (var column in ColumnsOutput) {
                if (column != ColumnsOutput[0]) {
                    builder.Append(", ");
                }
                builder.Append(column);
            }
            builder.Append(" FROM ");
            
            foreach (var table in Tables) {
                if (table != Tables[0]) {
                    builder.Append(", ");
                }
                builder.Append(table);
            }
            
            foreach (var @join in Joins) {
                builder.Append(" ");
                builder.Append(@join);
            }
            
            if (WhereClause != null) {
                builder.Append(" WHERE ");
                if (Configuration.DatabaseProvider.SupportsParameter) {
                    builder.Append(WhereClause.ToParametrizedString(Parameters));
                } else {
                    builder.Append(WhereClause);
                }
            }
            
            if (OrderBy.Count > 0) {
                builder.Append(" ORDER BY ");
                foreach (var item in OrderBy) {
                    if (item != OrderBy[0]) {
                        builder.Append(", ");
                    }
                    builder.Append(item);
                }
            }
            
            return builder.ToString();
        }

        public override int GetHashCode() {
            var code = Take.GetHashCode();
            code ^= Skip.GetHashCode();
            code ^= Tables.GetHashCode();
            code ^= ColumnsOutput.GetHashCode();
            code ^= OrderBy.GetHashCode();
            code ^= Joins.GetHashCode();
            code ^= WhereClause != null ? WhereClause.GetHashCode() : 1;
            return code;
        }

        public bool Equals(SqlQuery obj) {
            var @equals = true;
            @equals &= Take.Equals(obj.Take);
            @equals &= Skip.Equals(obj.Skip);
            @equals &= CompareList(Tables, obj.Tables);
            @equals &= CompareList(ColumnsOutput, obj.ColumnsOutput);
            @equals &= CompareList(OrderBy, obj.OrderBy);
            @equals &= CompareList(Joins, obj.Joins);
            @equals &= ((WhereClause != null && WhereClause.Equals(obj.WhereClause)) || (WhereClause == null && obj.WhereClause == null));
            return @equals;
        }

        private bool CompareList(IList list1, IList list2) {
            if (list1.Count != list2.Count)
                return false;
            
            var @equals = true;
            for (var i = 0; i < list1.Count; i++) {
                @equals &= list1[i].Equals(list2[i]);
            }
            return @equals;
        }

        public override bool Equals(object obj) {
            return Equals((SqlQuery)obj);
        }
    }
}
