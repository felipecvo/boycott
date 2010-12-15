namespace Boycott.Migrate {
    using System;

    public class MigrationProxy : IComparable<MigrationProxy> {
        public string Name { get; set; }
        public long Version { get; set; }
        public Type ClassType { get; set; }
        private Migration migration;
        protected Migration Migration {
            get { return migration ?? (migration = InitializeMigration()); }
        }

        private Migration InitializeMigration() {
            var item = (Migration)Activator.CreateInstance(ClassType);
            item.Proxy = this;
            return item;
        }

        #region IComparable<MigrationProxy> Members

        public int CompareTo(MigrationProxy other) {
            return Version.CompareTo(other.Version);
        }

        #endregion

        public void Migrate(MigrationDirection direction) {
            Migration.Migrate(direction);
        }

        public void Announce(string message) {
            Migration.Announce(message);
        }

        public void Write(string message) {
            Migration.Write(message);
        }
    }
}
