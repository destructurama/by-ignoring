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

using System.Linq.Expressions;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests;

public record ByIgnoringTestCase<TDestructure>(string TestName)
{
    public Expression<Func<TDestructure, object?>>[] IgnoredProperties { get; set; } = null!;
    public object? ObjectToDestructure { get; set; }
    public IDictionary<string, LogEventPropertyValue> ExpectedPropertiesLogged { get; set; } = null!;
}

public record ByIgnoreExceptionTestCase<TDestructure>(string TestName)
{
    public Expression<Func<TDestructure, object?>>[] IgnoredProperties { get; set; } = null!;
    public Type? ExceptionType { get; set; }
}

public static class ByIgnoringTestCases
{
    public static IEnumerable<ByIgnoringTestCase<DestructureMe>> DestructureMeSuccessTestCases()
    {
        yield return new ByIgnoringTestCase<DestructureMe>("Ignore id and password should only include name")
        {
            IgnoredProperties =
            [
                dm => dm.Id, // value type property
                dm => dm.Password, // reference type property
            ],
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

        yield return new ByIgnoringTestCase<DestructureMe>("Ignore id and password should only include name even if it is null")
        {
            IgnoredProperties =
            [
                dm => dm.Id, // value type property
                dm => dm.Password, // reference type property
            ],
            ObjectToDestructure = new DestructureMe
            {
                Id = 2,
                Name = null,
                Password = "Password",
            },
            ExpectedPropertiesLogged = new Dictionary<string, LogEventPropertyValue>
            {
                { "Name", ScalarValue.Null },
            },
        };

        yield return new ByIgnoringTestCase<DestructureMe>("Ignore just id should include two others")
        {
            IgnoredProperties =
            [
                dm => dm.Id, // value type property
            ],
            ObjectToDestructure = new DestructureMe
            {
                Id = 2,
                Name = "CoolName",
                Password = "Password",
            },
            ExpectedPropertiesLogged = new Dictionary<string, LogEventPropertyValue>
            {
                { "Name", new ScalarValue("CoolName") },
                { "Password", new ScalarValue("Password") },
            },
        };

        yield return new ByIgnoringTestCase<DestructureMe>("Ignore just password should include two others")
        {
            IgnoredProperties =
            [
                dm => dm.Password, // reference type property
            ],
            ObjectToDestructure = new DestructureMe
            {
                Id = 2,
                Name = "CoolName",
                Password = "Password",
            },
            ExpectedPropertiesLogged = new Dictionary<string, LogEventPropertyValue>
            {
                { "Id", new ScalarValue(2) },
                { "Name", new ScalarValue("CoolName") },
            },
        };

        yield return new ByIgnoringTestCase<DestructureMe>("Ignoring all properties should produce empty object")
        {
            IgnoredProperties =
            [
                dm => dm.Password,
                dm => dm.Name,
                dm => dm.Id,
            ],
            ObjectToDestructure = new DestructureMe
            {
                Id = 2,
                Name = "CoolName",
                Password = "Password",
            },
            ExpectedPropertiesLogged = new Dictionary<string, LogEventPropertyValue>(),
        };

        yield return new ByIgnoringTestCase<DestructureMe>("Destructure policy shouldn't come into play for other types")
        {
            IgnoredProperties =
            [
                dm => dm.Password, // reference type property
            ],
            ObjectToDestructure = new SomeOtherType
            {
                FullName = "Darth Vadar",
            },
            ExpectedPropertiesLogged = new Dictionary<string, LogEventPropertyValue>
            {
                { "FullName", new ScalarValue("Darth Vadar") },
            },
        };
    }

    public static IEnumerable<ByIgnoringTestCase<DestructureMeWithPropertyWithOnlySetter>> OnlySetterSuccessTestCases()
    {
        yield return new ByIgnoringTestCase<DestructureMeWithPropertyWithOnlySetter>("ClassWithAPropertyOnlyWithSetterDoesNotCrash")
        {
            IgnoredProperties =
            [
                dm => dm.Id, // value type property
                dm => dm.Password, // reference type property
            ],
            ObjectToDestructure = new DestructureMe
            {
                Id = 2,
                Name = "CoolName",
                Password = "Password",
            },
            // TODO - is this really the behavior we want? Leaving as is for now to be functionally equivalent, but it
            // would seem to me that we would either want to throw an exception or actually ignore Id and Password as the consumer intends.
            ExpectedPropertiesLogged = new Dictionary<string, LogEventPropertyValue>
            {
                { "Id", new ScalarValue(2) },
                { "Name", new ScalarValue("CoolName") },
                { "Password", new ScalarValue("Password") },
            },
        };
    }

    public static IEnumerable<ByIgnoreExceptionTestCase<DestructureMe>> ShouldThrowExceptionTestCases()
    {
        yield return new ByIgnoreExceptionTestCase<DestructureMe>("ComplexExpressionsFail")
        {
            IgnoredProperties =
            [
                dm => new
                {
                    Name = dm.Name,
                }
            ],
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<DestructureMe>("MethodExpressionsFail")
        {
            IgnoredProperties =
            [
                dm => dm.ToString(),
            ],
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<DestructureMe>("StringLiteralExpressionsFail")
        {
            IgnoredProperties =
            [
                dm => "string literal",
            ],
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<DestructureMe>("ChainedPropertyExpressionsFail")
        {
            IgnoredProperties =
            [
                dm => dm.Password!.Length
            ],
            ExceptionType = typeof(ArgumentException),
        };
    }

    public class DestructureMeWithPropertyWithOnlySetter
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string OnlySetter { set { } }
    }

    public class SomeOtherType
    {
        public string? FullName { get; set; }
    }
}
