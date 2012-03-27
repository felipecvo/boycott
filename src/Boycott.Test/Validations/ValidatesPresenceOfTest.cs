namespace Boycott.Test.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using Boycott.Validations;

    [TestFixture]
    public class ValidatesPresenceOfTest {
        [Test]
        public void ShouldBeIValidator() {
            var validator = new ValidatesPresenceOf() as IValidator;

            Assert.IsNotNull(validator);
        }

        [Test]
        public void ShouldValidateString() {
            var validator = new ValidatesPresenceOf();
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");
            dummy.DummyText = "Dummy dummy";

            Assert.IsTrue(validator.Validate(dummy, property));
        }

        [Test]
        public void ShouldFailStringValidation() {
            var validator = new ValidatesPresenceOf();
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");

            dummy.DummyText = null;
            Assert.IsFalse(validator.Validate(dummy, property));

            dummy.DummyText = string.Empty;
            Assert.IsFalse(validator.Validate(dummy, property));
        }

        [Test]
        public void ShouldHaveDefaultMessage() {
            var validator = new ValidatesPresenceOf();
            Assert.AreEqual("{0} can't be blank", validator.Message);
        }

        [Test]
        public void ShouldCustomizeMessage() {
            var validator = new ValidatesPresenceOf();
            validator.Message = "{0} não pode ser vazio";
            Assert.AreEqual("{0} não pode ser vazio", validator.Message);
        }

        [Test]
        public void ShouldFormatMessageWhenValidationFail() {
            var validator = new ValidatesPresenceOf();
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");

            validator.Validate(dummy, property);

            Assert.AreEqual("DummyText can't be blank", validator.Message);
        }

        [Test]
        public void ShouldEmptyMessageWhenValidationSucceed() {
            var validator = new ValidatesPresenceOf();
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");
            validator.Validate(dummy, property);
            dummy.DummyText = "dummy dummy";
            validator.Validate(dummy, property);

            Assert.IsNullOrEmpty(validator.Message);
        }

        [Test]
        public void ShouldFormatMessageWhenValidationFailAgain() {
            var validator = new ValidatesPresenceOf();
            var dummy = new VeryDummyClass();

            validator.Validate(dummy, dummy.GetType().GetProperty("DummyText"));
            validator.Validate(dummy, dummy.GetType().GetProperty("DummyProperty"));

            Assert.AreEqual("DummyProperty can't be blank", validator.Message);
        }

        [Test]
        public void ShouldFormatCustomMessageWhenValidationFail() {
            var validator = new ValidatesPresenceOf();
            validator.Message = "{0} can't be empty";
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");

            validator.Validate(dummy, property);

            Assert.AreEqual("DummyText can't be empty", validator.Message);
        }

        [Test]
        public void ShouldFormatCustomMessageWhenValidationFailAgain() {
            var validator = new ValidatesPresenceOf();
            validator.Message = "{0} can't be empty";
            var dummy = new VeryDummyClass();

            validator.Validate(dummy, dummy.GetType().GetProperty("DummyText"));
            validator.Validate(dummy, dummy.GetType().GetProperty("DummyProperty"));

            Assert.AreEqual("DummyProperty can't be empty", validator.Message);
        }

        class VeryDummyClass {
            public string DummyText { get; set; }
            public string DummyProperty { get; set; }
        }
    }
}
