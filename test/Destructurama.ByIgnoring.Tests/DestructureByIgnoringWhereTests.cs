using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Destructurama.ByIgnoring.Tests.Support;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests
{
    [TestFixture]
    public class DestructureByIgnoringWhereTests
    {
        [TestCaseSource(nameof(GetTestCases))]
        public void PropertyNamesInExpressionsAreIgnoredWhenDestructuring(TestCase testCase)
        {
            // Setup
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByIgnoringPropertiesWhere(testCase.HandleDestructuringPredicate, testCase.IgnoredPropertyPredicates)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            // Execute
            log.Information("Here is {@Ignored}", testCase.ObjectToDestructure);

            // Verify
            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            props.Should().BeEquivalentTo(testCase.ExpectedPropertiesLogged);
        }

        private static IEnumerable<TestCase> GetTestCases()
        {
            yield return new TestCase("Ignore id and password should only include name")
            {
                HandleDestructuringPredicate = obj => obj is IDestructureMe,
                IgnoredPropertyPredicates = new Func<PropertyInfo, bool>[]
                {
                    pi => pi.Name == nameof(DestructureMe.Id),
                    pi => pi.Name == nameof(DestructureMe.Password),
                },
                ObjectToDestructure = new DestructureMe
                {
                    Id = 2,
                    Name = "CoolName",
                    Password = "Password",
                },
                ExpectedPropertiesLogged = new Dictionary<string, LogEventPropertyValue>
                {
                    { "Name", new ScalarValue("CoolName") },
                },
            };
        }

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

        public record TestCase(string TestName)
        {
            public Func<object, bool> HandleDestructuringPredicate { get; set; }
            public Func<PropertyInfo, bool>[] IgnoredPropertyPredicates { get; set; }
            public object ObjectToDestructure { get; set; }
            public IDictionary<string, LogEventPropertyValue> ExpectedPropertiesLogged { get; set; }
        }
    }
}
