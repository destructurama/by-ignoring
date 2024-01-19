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
using System.Reflection;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests.TestCases;

public class ByIgnoreWhereTestCases
{
    public static IEnumerable<ByIgnoreWhereTestCase> ShouldDestructureSuccessfullyTestCases()
    {
        yield return new ByIgnoreWhereTestCase("given matching handle predicate, then ignore properties according to supplied predicates")
        {
            HandleDestructuringPredicate = obj => obj.GetType() == typeof(DestructureMe),
            IgnoredPropertyPredicates = new Func<PropertyInfo, bool>[]
            {
                pi => pi.Name == nameof(DestructureMe.Id), // value type property
                pi => pi.Name == nameof(DestructureMe.Password), // reference type property
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

        yield return new ByIgnoreWhereTestCase("given a non-matching handle predicate, then log entire object")
        {
            HandleDestructuringPredicate = obj => obj.GetType() == typeof(DestructureMe),
            IgnoredPropertyPredicates = new Func<PropertyInfo, bool>[]
            {
                pi => pi.Name == nameof(DestructureMe.Id), // value type property
                pi => pi.Name == nameof(DestructureMe.Password), // reference type property
            },
            ObjectToDestructure = new
            {
                Id = 2,
                Name = "CoolName",
                Password = "Password",
            },
            ExpectedPropertiesLogged = new Dictionary<string, LogEventPropertyValue>
            {
                { "Id", new ScalarValue(2) },
                { "Name", new ScalarValue("CoolName") },
                { "Password", new ScalarValue("Password") },
            },
        };

        foreach (var scenario in Convert(ByIgnoringTestCases.DestructureMeSuccessTestCases()))
        {
            yield return scenario;
        }

        foreach (var scenario in Convert(ByIgnoringTestCases.OnlySetterSuccessTestCases()))
        {
            yield return scenario;
        }
    }

    public static IEnumerable<ByIgnoreWhereExceptionTestCase> ShouldThrowExceptionTestCases()
    {
        yield return new ByIgnoreWhereExceptionTestCase("null handleDestructuringPredicate")
        {
            HandleDestructuringPredicate = null!,
            IgnoredPropertyPredicates = new Func<PropertyInfo, bool>[]
            {
                pi => pi.Name == nameof(DestructureMe.Id),
                pi => pi.Name == nameof(DestructureMe.Password),
            },
            ExceptionType = typeof(ArgumentNullException),
        };

        yield return new ByIgnoreWhereExceptionTestCase("null ignoredPropertyPredicates")
        {
            HandleDestructuringPredicate = obj => obj is IDestructureMe,
            IgnoredPropertyPredicates = null!,
            ExceptionType = typeof(ArgumentNullException),
        };

        yield return new ByIgnoreWhereExceptionTestCase("null handleDestructuringPredicate and null ignoredPropertyPredicates")
        {
            HandleDestructuringPredicate = null!,
            IgnoredPropertyPredicates = null!,
            ExceptionType = typeof(ArgumentNullException),
        };

        yield return new ByIgnoreWhereExceptionTestCase("empty ignoredPropertyPredicates")
        {
            HandleDestructuringPredicate = obj => obj is IDestructureMe,
            IgnoredPropertyPredicates = Array.Empty<Func<PropertyInfo, bool>>(),
            ExceptionType = typeof(ArgumentOutOfRangeException),
        };
    }

    private static IEnumerable<ByIgnoreWhereTestCase> Convert<T>(IEnumerable<ByIgnoringTestCase<T>> input)
    {
        return input
            .Select(x => new ByIgnoreWhereTestCase(x.TestName)
            {
                HandleDestructuringPredicate = obj => obj.GetType() == typeof(T),
                IgnoredPropertyPredicates = x.IgnoredProperties
                    .Select<Expression<Func<T, object?>>, Func<PropertyInfo, bool>>(
                        // It's also not great that we're using sut code - GetPropertyNameFromExpression() - in our test. This is edging towards tautological. But I've got nothing better at the moment other than duplicating scenarios
                        destructureMe => propertyInfo => propertyInfo.Name == destructureMe.GetPropertyNameFromExpression())
                    .ToArray(),
                ObjectToDestructure = x.ObjectToDestructure,
                ExpectedPropertiesLogged = x.ExpectedPropertiesLogged,
            });
    }
}

public record ByIgnoreWhereTestCase(string TestName)
{
    public Func<object, bool> HandleDestructuringPredicate { get; set; } = null!;
    public Func<PropertyInfo, bool>[] IgnoredPropertyPredicates { get; set; } = null!;
    public object? ObjectToDestructure { get; set; }
    public IDictionary<string, LogEventPropertyValue> ExpectedPropertiesLogged { get; set; } = null!;
}

public record ByIgnoreWhereExceptionTestCase(string TestName)
{
    public Func<object, bool> HandleDestructuringPredicate { get; set; } = null!;
    public Func<PropertyInfo, bool>[] IgnoredPropertyPredicates { get; set; } = null!;
    public Type? ExceptionType { get; set; }
}
