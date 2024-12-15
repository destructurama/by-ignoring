// Copyright 2017 Serilog Contributors
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

using FluentAssertions;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Shouldly;

namespace Destructurama.ByIgnoring.Tests;

[TestFixture]
public class DestructureByIgnoringTests
{
    [TestCaseSource(typeof(ByIgnoringTestCases), nameof(ByIgnoringTestCases.DestructureMeSuccessTestCases))]
    [TestCaseSource(typeof(ByIgnoringTestCases), nameof(ByIgnoringTestCases.OnlySetterSuccessTestCases))]
    public void PropertiesAreIgnoredWhenDestructuring<T>(ByIgnoringTestCase<T> testCase)
    {
        // Setup
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.ByIgnoringProperties(testCase.IgnoredProperties)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Execute
        log.Information("Here is {@Ignored}", testCase.ObjectToDestructure);

        // Verify
        var sv = (StructureValue)evt.Properties["Ignored"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.Should().BeEquivalentTo(testCase.ExpectedPropertiesLogged, options => options.UsingSerilogTypeComparisons());
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

    [Test]
    public void Throwing_Accessor_Should_Be_Handled()
    {
        // Setup
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.ByIgnoringProperties<DestructureMeThrows>(o => o.Id)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();
        var obj = new DestructureMeThrows();

        // Execute
        log.Information("Here is {@Ignored}", obj);

        // Verify
        var sv = (StructureValue)evt.Properties["Ignored"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("BadProperty");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe("The property accessor threw an exception: FormatException");
    }

    [Test]
    public void TryDestructure_Should_Return_False_When_Called_With_Null()
    {
        var policy = new DestructureByIgnoringPolicy(_ => true, (_, _) => false, _ => true);
        policy.TryDestructure(null!, null!, out _).ShouldBeFalse();
    }

    [Test]
    public void TryDestructure_Should_Work_For_Structs()
    {
        // Setup
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.ByIgnoringProperties<DestructureMeStruct>(o => o.Id)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();
        var obj = new DestructureMeStruct();

        // Execute
        log.Information("Here is {@Ignored}", obj);

        // Verify
        var sv = (StructureValue)evt.Properties["Ignored"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("Name");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe("Tom");
    }

    [Test]
    public void TryDestructure_Should_Ignore_Property_From_Options()
    {
        // Setup
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.ByIgnoring<DestructureMeClass>(o => o.Ignore(x => x.Id))
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();
        var obj = new DestructureMeClass { Id = 42 };

        // Execute
        log.Information("Here is {@Ignored}", obj);

        // Verify
        var sv = (StructureValue)evt.Properties["Ignored"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("Name");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe("Tom");
    }

    [Test]
    public void TryDestructure_Should_Ignore_Null_String()
    {
        // Setup
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.ByIgnoring<DestructureMeClass>(o => o.IgnoreValue((_, v) => v is null))
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();
        var obj = new DestructureMeClass { Name = null };

        // Execute
        log.Information("Here is {@Ignored}", obj);

        // Verify
        var sv = (StructureValue)evt.Properties["Ignored"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("Id");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe(0);
    }

    [Test]
    public void TryDestructure_Should_Ignore_Custom_Value()
    {
        // Setup
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.ByIgnoring<DestructureMeClass>(o => o.IgnoreValue((p, v) => p.Name is "Id" && v is 42))
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();
        var obj = new DestructureMeClass { Id = 42 };

        // Execute
        log.Information("Here is {@Ignored}", obj);

        // Verify
        var sv = (StructureValue)evt.Properties["Ignored"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("Name");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe("Tom");
    }

    [Test]
    public void TryDestructure_Should_Ignore_All_AssignableTo()
    {
        // Setup
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.ByIgnoring<IDestructureMe>(o => o.IgnoreValue((_, v) => v is null).DestructureAssignableTo())
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();
        var obj1 = new DestructureMeStruct { Name = null };
        var obj2 = new DestructureMeClass { Name = null };

        // Execute
        log.Information("Here is {@Ignored1} and {@Ignored2}", obj1, obj2);

        // Verify
        var sv = (StructureValue)evt.Properties["Ignored1"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("Id");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe(0);
        sv = (StructureValue)evt.Properties["Ignored2"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("Id");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe(0);
    }

    [Test]
    public void TryDestructure_Should_Ignore_Exact_Type()
    {
        // Setup
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.ByIgnoring<DestructureMeClass>(o => o.IgnoreValue((_, v) => v is null).DestructureExactType())
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();
        var obj1 = new DestructureMeStruct { Name = null };
        var obj2 = new DestructureMeClass { Name = null };

        // Execute
        log.Information("Here is {@Ignored1} and {@Ignored2}", obj1, obj2);

        // Verify
        var sv = (StructureValue)evt.Properties["Ignored1"];
        sv.Properties.Count.ShouldBe(2);
        sv.Properties[0].Name.ShouldBe("Id");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe(0);
        sv.Properties[1].Name.ShouldBe("Name");
        sv.Properties[1].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBeNull();

        sv = (StructureValue)evt.Properties["Ignored2"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("Id");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe(0);
    }

    public struct DestructureMeStruct : IDestructureMe
    {
        public DestructureMeStruct()
        {
        }

        public int Id { get; set; }

        public string? Name { get; set; } = "Tom";
    }

    public interface IDestructureMe
    {
    }

    public class DestructureMeClass : IDestructureMe
    {
        public int Id { get; set; }

        public string? Name { get; set; } = "Tom";
    }
}
