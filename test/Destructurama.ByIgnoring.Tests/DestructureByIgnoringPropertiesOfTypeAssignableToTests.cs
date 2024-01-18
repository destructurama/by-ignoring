﻿// Copyright 2017 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
    public class DestructureByIgnoringPropertiesOfTypeAssignableToTests
    {
        [TestCaseSource(typeof(ByIgnoringPropertiesOfTypeAssignableToTestCases), nameof(ByIgnoringPropertiesOfTypeAssignableToTestCases.IDestructureMeSuccessTestCases))]
        [TestCaseSource(typeof(ByIgnoringTestCases), nameof(ByIgnoringTestCases.OnlySetterSuccessTestCases))]
        [TestCaseSource(typeof(ByIgnoringTestCases), nameof(ByIgnoringTestCases.DestructureMeSuccessTestCases))] // a type should be assignable to itself, so these should all pass
        public void PropertiesAreIgnoredWhenDestructuring<T>(ByIgnoringTestCase<T> testCase)
        {
            // Setup
            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.ByIgnoringPropertiesOfTypeAssignableTo(testCase.IgnoredProperties)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            // Execute
            log.Information("Here is {@Ignored}", testCase.ObjectToDestructure);

            // Verify
            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            props.Should().BeEquivalentTo(testCase.ExpectedPropertiesLogged, options => options.UsingSerilogTypeComparisons());
        }

        [TestCaseSource(typeof(ByIgnoringPropertiesOfTypeAssignableToTestCases), nameof(ByIgnoringPropertiesOfTypeAssignableToTestCases.ShouldThrowExceptionTestCases))]
        public void ExceptionThrownWhenRegisteringDestructure<T>(ByIgnoreExceptionTestCase<T> testCase)
        {
            // Setup
            var config = new LoggerConfiguration();

            // Execute
            Action configureByIgnoringAction = () => config.Destructure.ByIgnoringPropertiesOfTypeAssignableTo(testCase.IgnoredProperties);

            // Verify
            configureByIgnoringAction
                .Should()
                .Throw<Exception>()
                .Where(ex => ex.GetType() == testCase.ExceptionType);
        }
    }
}
