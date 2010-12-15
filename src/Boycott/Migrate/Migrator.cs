namespace Boycott.Migrate {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Boycott.Provider;

    public enum MigrationDirection {
        Up,
        Down
    }

    public class UnknownMigrationVersionError : Exception {
        public UnknownMigrationVersionError(long version) : base(string.Format("No migration with version number {0}", version)) {
        }
    }

    public class IllegalMigrationNameError : Exception {
        public IllegalMigrationNameError(string className) : base(string.Format("Illegal name for migration class: {0}\n\t(only letters, numbers, and '_' allowed)", className)) {
        }
    }

    public class DuplicateMigrationVersionError : Exception {
        public DuplicateMigrationVersionError(long version) : base(string.Format("Multiple migrations have the version number {0}", version)) {
        }
    }

    public class DuplicateMigrationNameError : Exception {
        public DuplicateMigrationNameError(string name) : base(string.Format("Multiple migrations have the name {0}", name)) {
        }
    }

    public class Migrator {
        #region Static

        public static void CreateDatabase() {
            try {
                ((MySQLProvider)Configuration.DatabaseProvider).CreateDatabase();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public static void DropDatabase() {
            try {
                ((MySQLProvider)Configuration.DatabaseProvider).DropDatabase();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public static void Migrate(long? targetVersion) {
            if (!targetVersion.HasValue) {
                Up(targetVersion);
            } else if (CurrentVersion > targetVersion) {
                Down(targetVersion);
            } else {
                Up(targetVersion);
            }
        }

        public static void Rollback(int steps) {
            var migrator = new Migrator(MigrationDirection.Down, null);
            var startIndex = migrator.Migrations.FindIndex(x => x.Version == migrator.LastMigrated);
            
            if (startIndex == 0)
                return;
            
            var finish = migrator.Migrations[startIndex + steps];
            Down(finish != null ? finish.Version : 0);
        }

        public static void Up(long? targetVersion) {
            (new Migrator(MigrationDirection.Up, targetVersion)).Migrate();
        }

        public static void Down(long? targetVersion) {
            (new Migrator(MigrationDirection.Down, targetVersion)).Migrate();
        }

        public static void Run(string direction, string migrationsPath, string targetVersion) {
            throw new NotImplementedException();
        }

        public static List<long> GetAllVersions {
            get {
                var list = new List<long>();
                using (var reader = Base.Provider.ExecuteReader(string.Format("SELECT version FROM {0}", Schema.SchemaMigrationsTableName))) {
                    while (reader.Read()) {
                        list.Add(reader.GetInt64(0));
                    }
                }
                return list;
            }
        }

        public static long CurrentVersion {
            get {
                var smTable = Schema.SchemaMigrationsTableName;
                if (!string.IsNullOrEmpty(smTable)) {
                    return GetAllVersions.Count > 0 ? GetAllVersions.Max() : 0;
                } else {
                    return 0;
                }
            }
        }

        public static string ProperTableName(string name) {
            //# Use the Active Record objects own table_name, or pre/suffix from ActiveRecord::Base if name is a symbol/string
            //name.table_name rescue "#{ActiveRecord::Base.table_name_prefix}#{name}#{ActiveRecord::Base.table_name_suffix}"
            //return name;
            throw new NotImplementedException();
        }

        #endregion

        MigrationDirection Direction { get; set; }
        long? TargetVersion { get; set; }
        List<MigrationProxy> migrations;

        public Migrator(MigrationDirection direction, long? targetVersion) {
            if (!Base.Provider.SupportsMigrations) {
                throw new Exception("This database does not yet support migrations");
            }
            Schema.InitializeSchemaMigrationsTable();
            Direction = direction;
            TargetVersion = targetVersion;
        }

        public long LastMigrated {
            get { return Migrated.Count > 0 ? Migrated.Last() : 0; }
        }

        public MigrationProxy CurrentMigration {
            get { return Migrations.Detect(m => m.Version == LastMigrated); }
        }

        public void Run() {
            throw new NotImplementedException();
            //  target = migrations.detect { |m| m.version == @target_version }
            //  raise UnknownMigrationVersionError.new(@target_version) if target.nil?
            //  unless (up? && migrated.include?(target.version.to_i)) || (down? && !migrated.include?(target.version.to_i))
            //    target.migrate(@direction)
            //    record_version_state_after_migrating(target.version)
            //  end
        }

        public void Migrate() {
            var current = Migrations.Detect(m => m.Version == LastMigrated);
            var target = Migrations.Detect(m => m.Version == TargetVersion);
            
            if (target == null && TargetVersion.HasValue && TargetVersion.Value > 0) {
                throw new UnknownMigrationVersionError(TargetVersion.Value);
            }
            
            var start = IsUp ? 0 : migrations.IndexOf(current);
            if (start == -1)
                start = 0;
            var finish = migrations.IndexOf(target);
            if (finish == -1)
                finish = migrations.Count > 0 ? migrations.Count : 1;
            if (start == 0 && finish == 0)
                finish = 1;
            var runnable = migrations.GetRange(start, finish - start);
            
            // skip the last migration if we're headed down, but not ALL the way down
            if (IsDown && target != null) {
                runnable.RemoveLast();
            }
            
            foreach (var migration in runnable) {
                // Base.logger.info "Migrating to #{migration.name} (#{migration.version})"
                
                // On our way up, we skip migrating the ones we've already migrated
                if (IsUp && Migrated.Contains(migration.Version)) {
                    continue;
                }
                
                // On our way down, we skip reverting the ones we've never migrated
                if (IsDown && !Migrated.Contains(migration.Version)) {
                    migration.Announce("never migrated, skipping");
                    migration.Write("");
                    continue;
                }
                
                try {
                    //      ddl_transaction do
                    migration.Migrate(Direction);
                    RecordVersionStateAfterMigrating(migration.Version);
                    //      end
                } catch (Exception e) {
                    //      canceled_msg = Base.connection.supports_ddl_transactions? ? "this and " : ""
                    //      raise StandardError, "An error has occurred, #{canceled_msg}all later migrations canceled:\n\n#{e}", e.backtrace
                    throw new Exception(string.Format("An error has occurred, all later migrations canceled:\n\n{0}", e));
                }
            }
        }

        public List<MigrationProxy> Migrations {
            get { return migrations ?? (GetMigrations()); }
        }

        private List<MigrationProxy> GetMigrations() {
            migrations = new List<MigrationProxy>();
            LoadAssemblies();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                foreach (var type in assembly.GetTypes()) {
                    if (type.IsSubclassOf(typeof(Migration)) && type.Name != "_001_Create_SchemaMigrations") {
                        var migration = new MigrationProxy();
                        
                        var match = Regex.Match(type.Name, "_([0-9]+)_([_a-zA-Z0-9]*)");
                        if (!match.Success)
                            throw new IllegalMigrationNameError(type.Name);
                        
                        migration.Version = long.Parse(match.Groups[1].Value);
                        migration.Name = match.Groups[2].Value;
                        migration.ClassType = type;
                        
                        if (migrations.Detect(m => m.Version == migration.Version) != null) {
                            throw new DuplicateMigrationVersionError(migration.Version);
                        }
                        
                        if (migrations.Detect(m => m.Name == migration.Name) != null) {
                            throw new DuplicateMigrationNameError(migration.Name);
                        }
                        
                        migrations.Add(migration);
                    }
                }
            }
            migrations.Sort();
            if (IsDown) {
                migrations.Reverse();
            }
            return migrations;
        }

        private void LoadAssemblies() {
            foreach (var item in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")) {
                System.Reflection.Assembly.LoadFile(item);
            }
            var path = Path.Combine(Environment.CurrentDirectory, "bin");
            if (Directory.Exists(path)) {
                foreach (var item in Directory.GetFiles(path, "*.dll")) {
                    System.Reflection.Assembly.LoadFile(item);
                }
            }
            path = Path.Combine(Environment.CurrentDirectory, Path.Combine("bin", "Debug"));
            if (Directory.Exists(path)) {
                foreach (var item in Directory.GetFiles(path, "*.dll")) {
                    System.Reflection.Assembly.LoadFile(item);
                }
            }
            path = Path.Combine(Environment.CurrentDirectory, Path.Combine("bin", "Release"));
            if (Directory.Exists(path)) {
                foreach (var item in Directory.GetFiles(path, "*.dll")) {
                    System.Reflection.Assembly.LoadFile(item);
                }
            }
        }

        public List<MigrationProxy> PendingMigrations {
            get {
                var alreadyMigrated = Migrated;
                Migrations.Reject(x => alreadyMigrated.Contains(x.Version));
                return Migrations;
            }
        }

        public List<long> Migrated {
            get { return migratedVersions ?? (migratedVersions = GetAllVersions); }
        }

        private List<long> migratedVersions;

        private void RecordVersionStateAfterMigrating(long version) {
            var smTable = Schema.SchemaMigrationsTableName;
            string sql;
            migratedVersions = migratedVersions ?? new List<long>();
            if (IsDown) {
                migratedVersions.Remove(version);
                sql = string.Format("DELETE FROM {0} WHERE version = '{1}'", smTable, version);
            } else {
                migratedVersions.Add(version);
                migratedVersions.Sort();
                sql = string.Format("INSERT INTO {0} (version) VALUES ('{1}')", smTable, version);
            }
            Configuration.DatabaseProvider.ExecuteNonQuery(sql);
        }

        private bool IsUp {
            get { return Direction == MigrationDirection.Up; }
        }

        private bool IsDown {
            get { return Direction == MigrationDirection.Down; }
        }
        
        //private
        //  # Wrap the migration in a transaction only if supported by the adapter.
        //  def ddl_transaction(&block)
        //    if Base.connection.supports_ddl_transactions?
        //      Base.transaction { block.call }
        //    else
        //      block.call
        //    end
        //  end
    }
    public static class X {
        public static T Detect<T>(this IList<T> list, Func<T, bool> function) {
            foreach (var item in list) {
                if (function(item)) {
                    return item;
                }
            }
            return default(T);
        }

        public static void Reject<T>(this IList<T> list, Func<T, bool> function) {
            for (int i = list.Count - 1; i >= 0; i--) {
                if (function(list[i])) {
                    list.RemoveAt(i);
                }
            }
        }

        public static void RemoveLast<T>(this List<T> list) {
            var lastIndex = list.Count - 1;
            list.RemoveAt(lastIndex);
        }
    }
    
}
