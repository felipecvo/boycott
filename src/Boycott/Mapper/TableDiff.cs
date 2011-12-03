namespace Boycott.Mapper {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Boycott.Helpers;

    public class TableDiff {
        public TableDiff(string tableName) {
            TableName = FixGenericsName(tableName);
            RemovedColumns = new List<DbColumn>();
            AddedColumns = new List<DbColumn>();
        }

        private string FixGenericsName(string name)
        {
            int backtick = name.IndexOf('`');
            if (backtick > -1)
                name = name.Substring(0, backtick);

            return name;
        }

        public string TableName { get; private set; }
        public bool NewTable { get; set; }
        public List<DbColumn> RemovedColumns { get; set; }
        public List<DbColumn> AddedColumns { get; set; }

        public List<string> ToSql() {
            var sb = new StringBuilder();
            var list = new List<string>();
            
            if (RemovedColumns.Count > 0) {
                foreach (var item in RemovedColumns) {
                    list.Add(string.Format("ALTER TABLE {0} DROP COLUMN {1}", TableName, item.Name));
                }
            }
            
            if (NewTable) {
                sb.AppendFormat("CREATE TABLE {0} (", TableName);
            } else {
                sb.AppendFormat("ALTER TABLE {0} ADD COLUMN (", TableName);
            }
            
            sb.Append(string.Join(",", AddedColumns.ConvertAll<string>(x => x.ToString()).ToArray()));
            
            sb.Append(")");
            
            list.Add(sb.ToString());
            
            if (NewTable && AddedColumns.FindAll(x => x.IsPrimaryKey).Count > 0) {
                list.Add(string.Format("ALTER TABLE {0} ADD PRIMARY KEY ({1})", TableName, string.Join(",", AddedColumns.FindAll(x => x.IsPrimaryKey).ConvertAll<string>(x => x.Name).ToArray())));
            }
            
            return list;
        }

        public override string ToString() {
            return string.Format("{2}+{0} added columns, -{1} removed columns", AddedColumns.Count, RemovedColumns.Count, NewTable ? "new table: " : "");
        }
    }
}
