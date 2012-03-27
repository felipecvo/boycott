namespace Boycott.Test.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using Boycott.Validations;

    [TestFixture]
    public class ValidatorTest {
        [Test]
        public void ShouldBeGenerics() {
            var validator = new Validator<DummyValidator>();

            Assert.AreEqual(typeof(DummyValidator), validator.Type);
        }

        [Test]
        public void ShouldHaveErrorsAfterValidationFailed() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            Assert.IsFalse(validator.Validate(dummy, "Name"));
            Assert.IsTrue(validator.HasErrors);
        }

        [Test]
        public void ShouldNotHaveErrorsAfterValidationSucceed() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            dummy.Name = "Josh";

            Assert.IsTrue(validator.Validate(dummy, "Name"));
            Assert.IsFalse(validator.HasErrors);
        }

        [Test]
        public void ShouldNotHaveErrorsAfterValidationFailAndSucceed() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            Assert.IsFalse(validator.Validate(dummy, "Name"));
            Assert.IsTrue(validator.HasErrors);

            dummy.Name = "Josh";

            Assert.IsTrue(validator.Validate(dummy, "Name"));
            Assert.IsFalse(validator.HasErrors);
        }

        [Test]
        public void ShouldNotHaveErrorsBeforeValidation() {
            var validator = new Validator<DummyValidator>();

            Assert.IsFalse(validator.HasErrors);
        }

        [Test]
        public void ShouldHavePropertiesToValidate() {
            var validator = new Validator<DummyValidator>();
            Assert.IsTrue(validator.Properties.ContainsKey("Name"));
        }

        [Test]
        public void ShouldNotHaveGetPropertiesToValidate() {
            var validator = new Validator<DummyValidator>();
            Assert.IsFalse(validator.Properties.ContainsKey("AlwaysValid"));
        }

        [Test]
        public void ShouldNotHaveStaticPropertiesToValidate() {
            var validator = new Validator<DummyValidator>();
            Assert.IsFalse(validator.Properties.ContainsKey("Static"));
        }

        [Test]
        public void ShouldNotHaveProtectedPropertiesToValidate() {
            var validator = new Validator<DummyValidator>();
            Assert.IsFalse(validator.Properties.ContainsKey("Protected"));
        }

        [Test]
        public void ShouldNotHavePrivatePropertiesToValidate() {
            var validator = new Validator<DummyValidator>();
            Assert.IsFalse(validator.Properties.ContainsKey("Private"));
        }

        [Test]
        public void ShouldNotHaveInternalPropertiesToValidate() {
            var validator = new Validator<DummyValidator>();
            Assert.IsFalse(validator.Properties.ContainsKey("Internal"));
        }

        [Test]
        public void ShouldInvalidateProperty() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            Assert.IsFalse(validator.Validate(dummy, "Name"));
        }

        [Test]
        public void ShouldSetErrorMessageWhenPropertyValidationFailed() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            validator.Validate(dummy, "Name");

            Assert.AreEqual("Name can't be blank", (string)validator.ErrorMessage);
        }

        [Test]
        public void ShouldClearErrorMessageWhenPropertyValidationSucceed() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            validator.Validate(dummy, "Name");

            dummy.Name = "Jose";

            validator.Validate(dummy, "Name");

            Assert.IsNullOrEmpty((string)validator.ErrorMessage);
        }

        [Test]
        public void ShouldValidateProperty() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            dummy.Name = "Josh";

            Assert.IsTrue(validator.Validate(dummy, "Name"));
        }

        [Test]
        public void ShouldPassPropertiesWithNoValidations() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            Assert.IsTrue(validator.Validate(dummy, "Simple"));
        }

        [Test]
        public void ShouldRaiseExceptionInvalidColumnName() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            Assert.IsTrue(validator.Validate(dummy, "noName"));
        }

        [Test]
        public void ShouldValidateAll() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            dummy.Name = "Josh";
            dummy.Email = "drake@josh.com";

            Assert.IsTrue(validator.Validate(dummy));
            Assert.IsFalse(validator.HasErrors);
        }

        [Test]
        public void ShouldFailValidationWithOneError() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            dummy.Name = "Josh";

            Assert.IsFalse(validator.Validate(dummy));
            Assert.IsTrue(validator.HasErrors);
            Assert.AreEqual("Email can't be blank", (string)validator.ErrorMessage);
        }

        [Test]
        public void ShouldFailValidationWithAllErrors() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            Assert.IsFalse(validator.Validate(dummy));
            Assert.IsTrue(validator.HasErrors);
            Assert.IsTrue(validator.ErrorMessage.ToString().Contains("Name can't be blank"));
            Assert.IsTrue(validator.ErrorMessage.ToString().Contains("Email can't be blank"));
        }

        [Test]
        public void ShouldFailValidationWithOneErrorAndPass() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            dummy.Name = "Josh";

            validator.Validate(dummy);

            dummy.Email = "my@email.com";

            Assert.IsTrue(validator.Validate(dummy));
            Assert.IsFalse(validator.HasErrors);
            Assert.IsNullOrEmpty((string)validator.ErrorMessage);
        }

        [Test]
        public void ShouldFailValidationWithAllErrorsAndPass() {
            var validator = new Validator<DummyValidator>();

            var dummy = new DummyValidator();

            validator.Validate(dummy);

            dummy.Name = "Me";
            dummy.Email = "my@email.com";

            Assert.IsTrue(validator.Validate(dummy));
            Assert.IsFalse(validator.HasErrors);
            Assert.IsNullOrEmpty((string)validator.ErrorMessage);
        }

        class DummyValidator {
            [ValidatesPresenceOf]
            public string Name { get; set; }
            [ValidatesPresenceOf]
            public string Email { get; set; }
            public string Simple { get; set; }

            public static string Static { get; set; }
            protected string Protected { get; set; }
            private string Private { get; set; }
            internal string Internal { get; set; }

            public bool AlwaysValid { get { return true; } }
        }
    }
}
