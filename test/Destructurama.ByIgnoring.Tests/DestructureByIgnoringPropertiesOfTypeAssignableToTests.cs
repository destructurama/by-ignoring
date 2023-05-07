using System;
using System.Linq;
using System.Linq.Expressions;
using Destructurama.ByIgnoring.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests
{
    [TestFixture]
    public class DestructureByIgnoringPropertiesOfTypeAssignableToTests
    {
        interface IDestructureMe
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
        }

        class DestructureMe : IDestructureMe
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
            public static string SomeStatic { get; set; } = "AAA";
            public string this[int index] => "value";
        }

        [Test]
        public void PropertyNamesInExpressionsAreIgnoredWhenDestructuring()
        {
            LogEvent evt = null;

            Expression<Func<IDestructureMe, object>> valueTypeProperty = dm => dm.Id;
            Expression<Func<IDestructureMe, object>> referenceTypeProperty = dm => dm.Password;

            var log = new LoggerConfiguration()
                .Destructure.ByIgnoringPropertiesOfTypeAssignableTo(valueTypeProperty, referenceTypeProperty)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var ignored = new DestructureMe
            {
                Id = 2,
                Name = "Name",
                Password = "Password"
            };

            log.Information("Here is {@Ignored}", ignored);

            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsFalse(props.ContainsKey("Id"), "Id property should have been ignored");
            Assert.IsFalse(props.ContainsKey("Password"), "Password property should have been ignored.");
            Assert.IsFalse(props.ContainsKey("SomeStatic"), "SomeStatic static property should have been always ignored.");
            Assert.AreEqual("Name", props["Name"].LiteralValue());
        }

        [Test]
        public void ComplexExpressionsFail()
        {
            AssertUnsupportedExpression<IDestructureMe>(dm => new
            {
                Name = dm.Name
            });
        }

        [Test]
        public void MethodExpressionsFail()
        {
            AssertUnsupportedExpression<IDestructureMe>(dm => dm.ToString());
        }

        [Test]
        public void StringLiteralExpressionsFail()
        {
            AssertUnsupportedExpression<IDestructureMe>(dm => "string literal");
        }

        [Test]
        public void ChainedPropertyExpressionsFail()
        {
            AssertUnsupportedExpression<IDestructureMe>(dm => dm.Password.Length);
        }

        private void AssertUnsupportedExpression<T>(Expression<Func<T, object>> expressionThatShouldFail)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                    new LoggerConfiguration()
                    .Destructure
                    .ByIgnoringPropertiesOfTypeAssignableTo(expressionThatShouldFail)
            );

            Assert.That(ex.ParamName, Is.EqualTo("ignoredProperty"));
        }


        class DestructureMeWithPropertyWithOnlySetter
        {
            private string _onlySetter;
            public int Id { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
            public string OnlySetter { set { _onlySetter = value; } }
        }

        [Test]
        public void ClassWithAPropertyOnlyWithSetterDoesNotCrash()
        {
            LogEvent evt = null;

            Expression<Func<DestructureMeWithPropertyWithOnlySetter, object>> valueTypeProperty = dm => dm.Id;
            Expression<Func<DestructureMeWithPropertyWithOnlySetter, object>> referenceTypeProperty = dm => dm.Password;

            var log = new LoggerConfiguration()
                .Destructure.ByIgnoringPropertiesOfTypeAssignableTo(valueTypeProperty, referenceTypeProperty)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var ignored = new DestructureMeWithPropertyWithOnlySetter
            {
                Id = 2,
                Name = "Name",
                Password = "Password"
            };

            log.Information("Here is {@Ignored}", ignored);

            Assert.IsTrue(true, "We did not throw!");
        }
    }
}
