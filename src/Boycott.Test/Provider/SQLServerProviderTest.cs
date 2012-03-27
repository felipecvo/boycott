namespace Boycott.Test.Provider {
    using NUnit.Framework;
    using Boycott.Provider;
    using System.Data;
    using System;

    [TestFixture]
    public class SQLServerProviderTest {
        [Test]
        public void ShouldSupportParameters() {
            var provider = new SQLServerProvider();
            Assert.True(provider.SupportsParameter);
        }

        [Test]
        public void ShouldConnectToDatabase() {
            var provider = new SQLServerProvider(".", "master", "sa", "P@ssw0rd");
            using (var conn = provider.GetConnection()) {
                Assert.IsNotNull(conn);
                Assert.AreEqual(ConnectionState.Open, conn.State);
            }
        }

        [Test]
        [ExpectedException]
        public void ShouldFailConnectToDatabase() {
            var provider = new SQLServerProvider(".", "nunit", "sa", "P@ssw0rd");
            using (var conn = provider.GetConnection()) {
                Assert.IsNotNull(conn);
                Assert.AreEqual(ConnectionState.Open, conn.State);
            }
        }

        [Test]
        public void ShouldCheckThatDatabaseExists() {
            var provider = new SQLServerProvider(".", "master", "sa", "P@ssw0rd");
            var exist = provider.DatabaseExists("master");
            Assert.IsTrue(exist);
        }

        [Test]
        public void ShouldCheckThatDatabaseDoesNotExists() {
            var provider = new SQLServerProvider(".", "nunit", "sa", "P@ssw0rd");
            var exist = provider.DatabaseExists("nunit");
            Assert.IsFalse(exist);
        }

        [Test]
        public void ShouldCreateDatabase() {
            var name = string.Format("nunit_{0:yyyyMMddHHmmss}", DateTime.Now);
            var provider = new SQLServerProvider(".", name, "sa", "P@ssw0rd");
            Assert.IsFalse(provider.DatabaseExists(name));
            provider.CreateDatabase();
            Assert.IsTrue(provider.DatabaseExists(name));
            Assert.IsTrue(true);
            GC.Collect();
            provider.DropDatabase();
        }

        [Test]
        public void ShouldDropDatabase() {
            var name = string.Format("nunit_{0:yyyyMMddHHmmss}", DateTime.Now);
            var provider = new SQLServerProvider(".", name, "sa", "P@ssw0rd");
            provider.CreateDatabase();
            provider.DropDatabase();
            Assert.IsFalse(provider.DatabaseExists(name));
        }

        [Test]
        public void ShouldGetTables() {
            var provider = new SQLServerProvider(".", "master", "sa", "P@ssw0rd");
            var tables = provider.Tables;
            Assert.Greater(tables.Count, 0);
        }

        [Test]
        public void ShouldGetAutoIncrement() {
             var provider = new SQLServerProvider(".", "master", "sa", "P@ssw0rd");
             Assert.AreEqual("IDENTITY(1,1)", provider.GetAutoIncrement());
        }

        [Test]
        public void ShouldGetColumns() {
            var provider = new SQLServerProvider(".", "", "sa", "P@ssw0rd");
            if (!provider.DatabaseExists("nunit_columns")) {
                provider = new SQLServerProvider(".", "nunit_columns", "sa", "P@ssw0rd");
                provider.CreateDatabase();
                if (provider.DatabaseExists("nunit_columns")) {
                    GC.Collect();
                    provider = new SQLServerProvider(".", "nunit_columns", "sa", "P@ssw0rd");

                    using (var conn = provider.GetConnection()) {
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = @"CREATE TABLE [nunit_columns].[dbo].[sample_table](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[display_name] [varchar](255) NULL,
	[name] [varchar](255) NULL,
	[ssn] [varchar](255) NOT NULL DEFAULT ('TBD'),
	[salary] [float](53) NOT NULL DEFAULT (0),
	[created_at] [datetime] NULL,
	[updated_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
))";
                        cmd.ExecuteNonQuery();
                    }
                    provider.Recycle();
                }
            } else {
                provider = new SQLServerProvider(".", "nunit_columns", "sa", "P@ssw0rd");
            }

            var table = provider.Tables[0];
            var columns = provider.GetColumns(table);
            Assert.Greater(columns.Count, 0);

            AssertColumn(columns[0], "id", true, null, true, 0, false, 0, 0, Migrate.DbType.Integer);
            AssertColumn(columns[1], "display_name", false, null, false, 255, true, 0, 0, Migrate.DbType.String);
            AssertColumn(columns[2], "name", false, null, false, 255, true, 0, 0, Migrate.DbType.String);
            AssertColumn(columns[3], "ssn", false, "('TBD')", false, 255, false, 0, 0, Migrate.DbType.String);
            AssertColumn(columns[4], "salary", false, "((0))", false, 0, false, 15, 8, Migrate.DbType.Float);
            AssertColumn(columns[5], "created_at", false, null, false, 0, true, 0, 0, Migrate.DbType.DateTime);
            AssertColumn(columns[6], "updated_at", false, null, false, 0, true, 0, 0, Migrate.DbType.DateTime);

            if (provider.DatabaseExists("nunit_columns")) {
                provider.DropDatabase();
            }
        }

        private void AssertColumn(Boycott.Helpers.DbColumn column, string name, bool increment, string defaultValue, bool pk, int limit,
            bool nullable, int precision, int scale, Migrate.DbType type) {
            Assert.AreEqual(column.Name, name, name);
            Assert.AreEqual(column.AutoIncrement, increment, "{0} - auto_increment", name);
            Assert.AreEqual(column.DefaultValue, defaultValue, "{0} - default", name);
            Assert.AreEqual(column.IsPrimaryKey, pk, "{0} - pk", name);
            Assert.AreEqual(column.Limit, limit, "{0} - limit", name);
            Assert.AreEqual(column.Nullable, nullable, "{0} - nullable", name);
            Assert.AreEqual(column.Precision, precision, "{0} - precision", name);
            Assert.AreEqual(column.Scale, scale, "{0} - scale", name);
            Assert.AreEqual(column.Type, type, "{0} - type", name);
        }
    }
}
