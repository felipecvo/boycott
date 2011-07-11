namespace Boycott.Console {
    using System;

    public class Author : Base<Author> {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

