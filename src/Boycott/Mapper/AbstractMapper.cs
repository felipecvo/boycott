namespace Boycott.Mapper {
    using System.Collections.Generic;
    using Boycott.Helpers;

    public abstract class AbstractMapper {
        public string TableName { get; set; }
        public List<DbColumn> Columns { get; set; }

        public override int GetHashCode() {
            return TableName.GetHashCode() ^ Columns.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is AbstractMapper))
                return false;
            
            return Equals((AbstractMapper)obj);
        }

        public bool Equals(AbstractMapper obj) {
            var eql = TableName.Equals(obj.TableName);
            
            eql &= Columns.Count.Equals(obj.Columns.Count);
            
            if (eql) {
                foreach (var item in Columns) {
                    var col = obj.Columns.Find(x => item.Equals(x));
                    if (col == null)
                        return false;
                }
            }
            
            return eql;
        }
    }
}
