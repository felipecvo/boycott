namespace Boycott.Test {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using Boycott.Validations;
    using System.ComponentModel;

    [TestFixture]
    public class BusinessBaseTest {
        [Test]
        public void ShouldValidate() {
            var business = new MyBusiness();
            business.Name = "Felipe";
            Assert.IsTrue(business.Validate());
        }

        [Test]
        public void ShouldFailValidate() {
            var business = new MyBusiness();

            Assert.IsFalse(business.Validate());
        }

        [Test]
        public void ShouldHaveErrors() {
            var business = new MyBusiness();

            business.Validate();

            Assert.IsTrue(business.HasErrors);
        }

        [Test]
        public void ShouldImplementIDataErrorInfo() {
            var business = new MyBusiness() as IDataErrorInfo;

            Assert.IsNotNull(business);
        }

        [Test]
        public void ShouldImplementIDataErrorInfo_Error() {
            var business = new MyBusiness();

            business.Validate();

            Assert.AreEqual("Name can't be blank", ((IDataErrorInfo)business).Error);
        }

        [Test]
        public void ShouldImplementIDataErrorInfo_Indexed_Success() {
            var business = new MyBusiness();

            business.Name = "Bill Gates";

            Assert.IsNullOrEmpty(((IDataErrorInfo)business)["Name"]);
        }

        [Test]
        public void ShouldImplementIDataErrorInfo_Indexed_Failed() {
            var business = new MyBusiness() as IDataErrorInfo;

            Assert.AreEqual("Name can't be blank", ((IDataErrorInfo)business)["Name"]);
        }

        class MyBusiness : BusinessBase<MyBusiness> {
            public int Id { get; set; }
            [ValidatesPresenceOf]
            public string Name { get; set; }
        }
    }
}
