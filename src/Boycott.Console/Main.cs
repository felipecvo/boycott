using System;
using System.Linq;
using Boycott.Provider;
using Boycott.Mapper;
using System.Collections.Generic;

namespace Boycott.Console {
    class MainClass {
        public static void Main(string[] args) {
            Configuration.DatabaseProvider = new MySQLProvider("localhost", "boycott", "root", "root");

            var sync = new Synchronizator();
            if (!sync.DatabaseExists) {
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

            foreach (Book b in Book.All())
            {
                Book.Delete(b.Id);
            }
            new Book() { Id = 1, Name = "O Senhor dos an�is" }.Save();
            new Book() { Id = 2, Name = "O Legado de Joran" }.Save();
            new Book() { Id = 3, Name = "C#" }.Save();
            new Book() { Id = 4, Name = "O Senhor dos an�is" }.Save();
            List<Book> books = Book.FindAll(new {Name = "O Legado de Joran"});
            Book book = Book.Find(2);
            List<Book> books_lotr = Book.FindAll(new { Name = "O Senhor dos an�is" } );

            System.Console.WriteLine("Hello World!");
            System.Console.Read();
        }
    }
}

