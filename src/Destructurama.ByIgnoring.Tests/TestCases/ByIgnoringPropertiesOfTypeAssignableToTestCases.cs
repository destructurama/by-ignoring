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

public class ByIgnoringPropertiesOfTypeAssignableToTestCases
{
    public static IEnumerable<ByIgnoringTestCase<IDestructureMe>> IDestructureMeSuccessTestCases()
    {
        // TODO - I can't figure out a way to convert these from ByIgnoringTestCases.DestructureMeSuccessTestCases(), so we'll duplicate the scenarios here and change the type. If someone can figure this out please make them more like ByIgnoringWhereTestCases.cs
        yield return new ByIgnoringTestCase<IDestructureMe>("Ignore id and password should only include name")
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

        yield return new ByIgnoringTestCase<IDestructureMe>("Ignore just id should include two others")
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

        yield return new ByIgnoringTestCase<IDestructureMe>("Ignore just password should include two others")
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

        yield return new ByIgnoringTestCase<IDestructureMe>("Ignoring all properties should produce empty object")
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

        yield return new ByIgnoringTestCase<IDestructureMe>("Destructure policy shouldn't come into play for other types")
        {
            IgnoredProperties =
            [
                dm => dm.Password, // reference type property
            ],
            ObjectToDestructure = new ByIgnoringTestCases.SomeOtherType
            {
                FullName = "Darth Vadar",
            },
            ExpectedPropertiesLogged = new Dictionary<string, LogEventPropertyValue>
            {
                { "FullName", new ScalarValue("Darth Vadar") },
            },
        };
    }

    public static IEnumerable<ByIgnoreExceptionTestCase<IDestructureMe>> ShouldThrowExceptionTestCases()
    {
        // TODO - I can't figure out a way to convert these from ByIgnoringTestCases.ShouldThrowExceptionTestCases(), so we'll duplicate the scenarios here and change the type. If someone can figure this out please make them more like ByIgnoringWhereTestCases.cs
        yield return new ByIgnoreExceptionTestCase<IDestructureMe>("ComplexExpressionsFail")
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

        yield return new ByIgnoreExceptionTestCase<IDestructureMe>("MethodExpressionsFail")
        {
            IgnoredProperties =
            [
                dm => dm.ToString(),
            ],
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<IDestructureMe>("StringLiteralExpressionsFail")
        {
            IgnoredProperties =
            [
                dm => "string literal",
            ],
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<IDestructureMe>("ChainedPropertyExpressionsFail")
        {
            IgnoredProperties =
            [
                dm => dm.Password!.Length
            ],
            ExceptionType = typeof(ArgumentException),
        };
    }
}
