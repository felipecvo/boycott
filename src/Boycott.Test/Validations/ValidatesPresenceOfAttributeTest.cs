namespace Boycott.Test.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using Boycott.Validations;

    [TestFixture]
    public class ValidatesPresenceOfAttributeTest {
        [Test]
        public void ShouldValidateAProperty() {
            var attr = typeof(DummmyValidatesPresenceOf).GetProperty("Name").GetCustomAttributes(typeof(ValidatesPresenceOfAttribute), false).First();
            Assert.IsNotNull(attr);
        }

        [Test]
        public void ShouldHaveDefaultMessage() {
            var attr = typeof(DummmyValidatesPresenceOf).GetProperty("Name").GetCustomAttributes(typeof(ValidatesPresenceOfAttribute), false).First() as ValidatesPresenceOfAttribute;
            Assert.AreEqual("{0} can't be blank", attr.Message);
        }

        [Test]
        public void ShouldCustomizeMessage() {
            var attr = typeof(DummmyValidatesPresenceOf).GetProperty("CustomName").GetCustomAttributes(typeof(ValidatesPresenceOfAttribute), false).First() as ValidatesPresenceOfAttribute;
            Assert.AreEqual("{0} não pode ser vazio", attr.Message);
        }
    }

    class DummmyValidatesPresenceOf {
        [ValidatesPresenceOf]
        public string Name { get; set; }

        [ValidatesPresenceOf(Message="{0} não pode ser vazio")]
        public string CustomName { get; set; }
    }
}
