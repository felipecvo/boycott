namespace Boycott.Test {
    using System;

    public class SampleTable : Base<SampleTable> {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public float Salary { get; set; }
        public DateTime Birthday { get; set; }
    }
}
