namespace Boycott.Migrate {
    static internal class Schema {
        public static string SchemaMigrationsTableName {
            get { return "schema_migrations"; }
        }

        public static void InitializeSchemaMigrationsTable() {
            var smTable = SchemaMigrationsTableName;
            
            if (!Migration.TableExists(smTable)) {
                using (var schemaMigrationsTable = Migration.CreateTable(smTable, new { Id = false })) {
                    schemaMigrationsTable.Column("version", DbType.Long, new { Null = false });
                }
                
                Migration.AddIndex(smTable, "version", new { Unique = true, Name = "unique_schema_migrations" });
            }
        }
    }
}
