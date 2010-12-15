namespace Boycott.Mapper {
    using System.Collections.Generic;
    using Boycott.Helpers;

    public class TableMapper : AbstractMapper {
        public TableMapper(string name) {
            TableName = name;
            
            if (TableExists) {
                Columns = Configuration.DatabaseProvider.GetColumns(TableName);
            } else {
                Columns = new List<DbColumn>();
            }
        }

        public bool TableExists {
            get { return Configuration.DatabaseProvider.Tables.Find(table => table == TableName) != null; }
        }
    }
}
