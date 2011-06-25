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
            var provider = new SQLServerProvider(".", "master", "sa", "P@ssw0rd");
            var table = provider.Tables[0];
            var columns = provider.GetColumns(table);
            Assert.Greater(columns.Count, 0);
        }
    }
}
