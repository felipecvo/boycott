namespace Boycott.Console {
    using System;

    public class Book : Base<Book> {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AuthorId { get; set; }
    }
}