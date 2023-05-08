using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Destructurama.ByIgnoring.Tests.Support;
using Destructurama.ByIgnoring.Tests.TestCases;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests
{
    [TestFixture]
    public class DestructureByIgnoringWhereTests
    {
        [TestCaseSource(typeof(ByIgnoreWhereTestCases), nameof(ByIgnoreWhereTestCases.ShouldDestructureSuccessfullyTestCases))]
        public void PropertiesAreIgnoredWhenDestructuring(ByIgnoreWhereTestCase testCase)
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

        [TestCaseSource(typeof(ByIgnoreWhereTestCases), nameof(ByIgnoreWhereTestCases.ShouldThrowExceptionTestCases))]
        public void ExceptionThrownWhenRegisteringDestructure(ByIgnoreWhereExceptionTestCase testCase)
        {
            // Setup
            var config = new LoggerConfiguration();

            // Execute
            Action configureByIgnoringAction = () => config.Destructure.ByIgnoringPropertiesWhere(testCase.HandleDestructuringPredicate, testCase.IgnoredPropertyPredicates);

            // Verify
            configureByIgnoringAction
                .Should()
                .Throw<Exception>()
                .Where(ex => ex.GetType() == testCase.ExceptionType);
        }
    }
}
