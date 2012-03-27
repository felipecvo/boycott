namespace Boycott.Test.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using Boycott.Validations;

    [TestFixture]
    public class CustomValidatorTest {
        [Test]
        public void ShouldBeIValidator() {
            var validator = new CustomValidator() as IValidator;

            Assert.IsNotNull(validator);
        }

        [Test]
        public void ShouldValidate() {
            var validator = new CustomValidator();
            validator.CustomMethod = SuccessMethod;
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");
            dummy.DummyText = "Dummy dummy";

            Assert.IsTrue(validator.Validate(dummy, property));
        }

        [Test]
        public void ShouldFailValidation() {
            var validator = new CustomValidator();
            validator.CustomMethod = FailMethod;
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");

            dummy.DummyText = "fail";
            Assert.IsFalse(validator.Validate(dummy, property));
        }

        [Test]
        public void ShouldDoNotHaveDefaultMessage() {
            var validator = new CustomValidator();
            Assert.IsNullOrEmpty(validator.Message);
        }

        [Test]
        public void ShouldCustomizeMessage() {
            var validator = new CustomValidator();
            validator.Message = "{0} can't be fail";
            Assert.AreEqual("{0} can't be fail", validator.Message);
        }

        [Test]
        public void ShouldDoNothingWhenValidationFail() {
            var validator = new CustomValidator();
            validator.CustomMethod = FailMethod;
            var dummy = new VeryDummyClass();
            dummy.DummyText = "fail";
            var property = dummy.GetType().GetProperty("DummyText");

            validator.Validate(dummy, property);

            Assert.IsNullOrEmpty(validator.Message);
        }

        [Test]
        public void ShouldFormatMessageWhenValidationFail() {
            var validator = new CustomValidator();
            validator.CustomMethod = FailMethod;
            validator.Message = "It can't be this";
            var dummy = new VeryDummyClass();
            dummy.DummyText = "fail";
            var property = dummy.GetType().GetProperty("DummyText");

            validator.Validate(dummy, property);

            Assert.AreEqual("It can't be this", validator.Message);
        }

        [Test]
        public void ShouldEmptyMessageWhenValidationSucceed() {
            var validator = new CustomValidator();
            validator.CustomMethod = FailMethod;
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");
            validator.Validate(dummy, property);
            validator.CustomMethod = SuccessMethod;
            validator.Validate(dummy, property);

            Assert.IsNullOrEmpty(validator.Message);
        }

        [Test]
        public void ShouldFormatMessageWhenValidationFailAgain() {
            var validator = new CustomValidator();
            validator.Message = "{0} can't be blank";
            validator.CustomMethod = FailMethod;
            var dummy = new VeryDummyClass();

            validator.Validate(dummy, dummy.GetType().GetProperty("DummyText"));
            validator.Validate(dummy, dummy.GetType().GetProperty("DummyProperty"));

            Assert.AreEqual("DummyProperty can't be blank", validator.Message);
        }

        [Test]
        public void ShouldFormatCustomMessageWhenValidationFail() {
            var validator = new CustomValidator();
            validator.CustomMethod = FailMethod;
            validator.Message = "{0} can't be empty";
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");

            validator.Validate(dummy, property);

            Assert.AreEqual("DummyText can't be empty", validator.Message);
        }

        [Test]
        public void ShouldFormatCustomMessageWhenValidationFailAgain() {
            var validator = new CustomValidator();
            validator.CustomMethod = FailMethod;
            validator.Message = "{0} can't be empty";
            var dummy = new VeryDummyClass();

            validator.Validate(dummy, dummy.GetType().GetProperty("DummyText"));
            validator.Validate(dummy, dummy.GetType().GetProperty("DummyProperty"));

            Assert.AreEqual("DummyProperty can't be empty", validator.Message);
        }

        [Test]
        public void ShouldValidateWithMethodName() {
            var validator = new CustomValidator();
            validator.CustomMethodName = "SuccessMethod";
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");
            dummy.DummyText = "Dummy dummy";

            Assert.IsTrue(validator.Validate(dummy, property));
        }

        [Test]
        public void ShouldFailValidationWithMethodName() {
            var validator = new CustomValidator();
            validator.CustomMethodName = "FailMethod";
            var dummy = new VeryDummyClass();
            var property = dummy.GetType().GetProperty("DummyText");

            dummy.DummyText = "fail";
            Assert.IsFalse(validator.Validate(dummy, property));
        }

        bool FailMethod(object value) {
            return false;
        }

        bool SuccessMethod(object value) {
            return true;
        }

        class VeryDummyClass {
            public string DummyText { get; set; }
            public string DummyProperty { get; set; }

            public bool FailMethod(object value) {
                return false;
            }

            public bool SuccessMethod(object value) {
                return true;
            }
        }
    }
}
