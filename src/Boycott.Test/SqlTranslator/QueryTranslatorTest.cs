namespace Boycott.Test.SqlTranslator {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NUnit.Framework;

    [TestFixture]
    public class QueryTranslatorTest {
        [Test]
        public void TestSimpleSelect() {
            Assert.IsNotNull(expression);
        }

        [Test]
        public void TestSqlJoin() {
            //svar expression = System.Linq.Expressions.
        }
    }
}
