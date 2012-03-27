namespace Boycott.Test.Validations {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using Boycott.Validations;

    [TestFixture]
    public class ErrorMessageStringCollectionTest {
        [Test]
        public void ShouldBeJustAString() {
            ErrorMessageStringCollection str = "rola?";
            Assert.AreEqual("rola?", (string)str);
        }

        [Test]
        public void ShouldAddMessageWithKey() {
            var erros = new ErrorMessageStringCollection();
            erros.Add("Name", "Name can't be blank");
            Assert.Pass();
        }

        [Test]
        public void ShouldAccessMessageWithKey() {
            var erros = new ErrorMessageStringCollection();
            erros.Add("Name", "Name can't be blank");
            Assert.AreEqual("Name can't be blank", erros["Name"]);
        }

        [Test]
        public void ShouldAccessMessagesWithKey() {
            var erros = new ErrorMessageStringCollection();
            erros.Add("Name", "Name can't be blank");
            erros.Add("Name", "Name can't be empty");
            Assert.AreEqual("Name can't be blank\nName can't be empty", erros["Name"]);
        }

        [Test]
        public void ShouldClearMessagesWithKey() {
            var errors = new ErrorMessageStringCollection();
            errors.Add("Name", "Name can't be blank");
            errors.Add("Name", "Name can't be empty");
            errors.Clear("Name");
            Assert.IsNull(errors["Name"]);
        }

        [Test]
        public void ShouldStringContainMessage() {
            var erros = new ErrorMessageStringCollection();
            erros.Add("Name", "Name can't be blank");
            Assert.AreEqual("Name can't be blank", (string)erros);
        }

        [Test]
        public void ShouldStringContainMessages() {
            var erros = new ErrorMessageStringCollection();
            erros.Add("Name", "Name can't be blank");
            Assert.AreEqual("Name can't be blank", (string)erros);
        }

        [Test]
        public void ShouldStringContainMessagesAndCaption() {
            ErrorMessageStringCollection errors = "There are some errors:";
            errors.Add("Name", "Name can't be blank");
            errors.Add("Name", "Name can't be empty");
            errors.Add("Email", "Email can't be blank");
            Assert.AreEqual("There are some errors:\nName can't be blank\nName can't be empty\nEmail can't be blank", (string)errors);
        }

        [Test]
        public void ShouldNotHaveErrors() {
            var errors = new ErrorMessageStringCollection();
            Assert.IsFalse(errors.HasErrors);
        }

        [Test]
        public void ShouldHaveErrors() {
            var errors = new ErrorMessageStringCollection();
            errors.Add("Name", "Name can't be blank");
            Assert.IsTrue(errors.HasErrors);
        }

        [Test]
        public void ShouldResetErrors() {
            var errors = new ErrorMessageStringCollection();
            errors.Add("Name", "Name can't be blank");
            errors.Clear("Name");
            Assert.IsFalse(errors.HasErrors);
        }
    }
}
