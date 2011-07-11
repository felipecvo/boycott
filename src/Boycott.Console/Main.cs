using System;
using System.Linq;
using Boycott.Provider;
using Boycott.Mapper;

namespace Boycott.Console {
    class MainClass {
        public static void Main(string[] args) {
            Configuration.DatabaseProvider = new MySQLProvider("localhost", "boycott", "root", "");

            var sync = new Synchronizator();
            if (!sync.DatabseExists) {
                Configuration.DatabaseProvider.CreateDatabase();
            }
            
            if (sync.Check()) {
                sync.Sync();
            }

            var expression = (from b in Book.db
                select b);

            if (expression == null) {
                System.Console.WriteLine("null");
            } else {
                System.Console.WriteLine(expression.GetType().Name);
            }
            
            expression = (from b in Book.db
                join a in Author.db on b.AuthorId equals a.Id
                select b);
            
            if (expression == null) {
                System.Console.WriteLine("null");
            } else {
                System.Console.WriteLine(expression.GetType().Name);
            }

            var list = expression.ToList();

            var total = Author.db.Count();

            System.Console.WriteLine("Hello World!");
        }
    }
}

