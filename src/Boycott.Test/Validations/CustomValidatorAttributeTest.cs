namespace Boycott.Test.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using Boycott.Validations;

    [TestFixture]
    public class CustomValidatorAttributeTest {
        [Test]
        public void ShouldValidateAProperty() {
            var attr = typeof(DummmyCustomValidatorAttribute).GetProperty("Name").GetCustomAttributes(typeof(CustomValidatorAttribute), false).First();
            Assert.IsNotNull(attr);
        }

        [Test]
        public void ShouldCustomizeMessage() {
            var attr = typeof(DummmyCustomValidatorAttribute).GetProperty("CustomName").GetCustomAttributes(typeof(CustomValidatorAttribute), false).First() as CustomValidatorAttribute;
            Assert.AreEqual("{0} não pode ser vazio", attr.Message);
        }

        [Test]
        public void ShouldPass() {
            var prop = typeof(DummmyCustomValidatorAttribute).GetProperty("Name");
            var obj = new DummmyCustomValidatorAttribute();
            var attr = prop.GetCustomAttributes(typeof(CustomValidatorAttribute), false).First() as CustomValidatorAttribute;
            Assert.IsTrue(attr.Validator.Validate(obj, prop));
        }

        [Test]
        public void ShouldFail() {
            var prop = typeof(DummmyCustomValidatorAttribute).GetProperty("CustomName");
            var obj = new DummmyCustomValidatorAttribute();
            var attr = prop.GetCustomAttributes(typeof(CustomValidatorAttribute), false).First() as CustomValidatorAttribute;
            Assert.IsFalse(attr.Validator.Validate(obj, prop));
        }

        class DummmyCustomValidatorAttribute {
            public bool DoFail(object value) { return false; }
            public bool DoPass(object value) { return true; }

            [CustomValidatorAttribute(ValidateMethod = "DoPass")]
            public string Name { get; set; }

            [CustomValidatorAttribute(Message = "{0} não pode ser vazio", ValidateMethod = "DoFail")]
            public string CustomName { get; set; }
        }
    }
}
