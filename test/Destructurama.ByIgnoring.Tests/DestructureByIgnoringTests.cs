using System;
using System.Linq;
using System.Linq.Expressions;
using Destructurama.ByIgnoring.Tests.Support;
using Destructurama.ByIgnoring.Tests.TestCases;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests
{
    [TestFixture]
    public class DestructureByIgnoringTests
    {
        [TestCaseSource(typeof(ByIgnoringTestCases), nameof(ByIgnoringTestCases.DestructureMeSuccessTestCases))]
        [TestCaseSource(typeof(ByIgnoringTestCases), nameof(ByIgnoringTestCases.OnlySetterSuccessTestCases))]
        public void PropertiesAreIgnoredWhenDestructuring<T>(ByIgnoringTestCase<T> testCase)
        {
            // Setup
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByIgnoringProperties(testCase.IgnoredProperties)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            // Execute
            log.Information("Here is {@Ignored}", testCase.ObjectToDestructure);

            // Verify
            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            props.Should().BeEquivalentTo(testCase.ExpectedPropertiesLogged);
        }

        [TestCaseSource(typeof(ByIgnoringTestCases), nameof(ByIgnoringTestCases.ShouldThrowExceptionTestCases))]
        public void ExceptionThrownWhenRegisteringDestructure<T>(ByIgnoreExceptionTestCase<T> testCase)
        {
            // Setup
            var config = new LoggerConfiguration();

            // Execute
            Action configureByIgnoringAction = () => config.Destructure.ByIgnoringProperties(testCase.IgnoredProperties);

            // Verify
            configureByIgnoringAction
                .Should()
                .Throw<Exception>()
                .Where(ex => ex.GetType() == testCase.ExceptionType);
        }
    }
}
