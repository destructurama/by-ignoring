// (C) Copyright 2023 Nullable, Inc. All rights reserved.
// This file is part of Nullable's product Aware and cannot be copied and/or
// distributed without the express permission of Nullable, Inc.

using System;
using System.Collections.Generic;
using System.Linq;
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

        foreach (var scenario in ConvertFromByIgnoringScenarios())
            yield return scenario;
    }

    public static IEnumerable<ByIgnoreWhereExceptionTestCase> ShouldThrowExceptionTestCases()
    {
        yield return new ByIgnoreWhereExceptionTestCase("null handleDestructuringPredicate")
        {
            HandleDestructuringPredicate = null,
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
            IgnoredPropertyPredicates = null,
            ExceptionType = typeof(ArgumentNullException),
        };

        yield return new ByIgnoreWhereExceptionTestCase("null handleDestructuringPredicate and null ignoredPropertyPredicates")
        {
            HandleDestructuringPredicate = null,
            IgnoredPropertyPredicates = null,
            ExceptionType = typeof(ArgumentNullException),
        };

        yield return new ByIgnoreWhereExceptionTestCase("empty ignoredPropertyPredicates")
        {
            HandleDestructuringPredicate = obj => obj is IDestructureMe,
            IgnoredPropertyPredicates = Array.Empty<Func<PropertyInfo, bool>>(),
            ExceptionType = typeof(ArgumentOutOfRangeException),
        };
    }

    private static IEnumerable<ByIgnoreWhereTestCase> ConvertFromByIgnoringScenarios()
    {
        // It isn't great that this test has knowledge of ByIgnoringTestCases's types, but this seems like an ok concession.
        if (ByIgnoringTestCases.DestructureMeSuccessTestCases().Any(x => x.MyType != typeof(DestructureMe)))
        {
            throw new ApplicationException(
                $"It looks like {nameof(ByIgnoringTestCases)}.{nameof(ByIgnoringTestCases.DestructureMeSuccessTestCases)} is returning a generic other than {nameof(DestructureMe)}." +
                $"In order to prevent duplicate tests we generate test cases assuming this type is used.");
        }

        return ByIgnoringTestCases.DestructureMeSuccessTestCases()
            .Select(x => new ByIgnoreWhereTestCase(x.TestName)
            {
                HandleDestructuringPredicate = obj => obj.GetType() == typeof(DestructureMe),
                IgnoredPropertyPredicates = x.IgnoredProperties
                    .Select<Expression<Func<DestructureMe, object>>, Func<PropertyInfo, bool>>(
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
    public Func<object, bool> HandleDestructuringPredicate { get; set; }
    public Func<PropertyInfo, bool>[] IgnoredPropertyPredicates { get; set; }
    public object ObjectToDestructure { get; set; }
    public IDictionary<string, LogEventPropertyValue> ExpectedPropertiesLogged { get; set; }
}

public record ByIgnoreWhereExceptionTestCase(string TestName)
{
    public Func<object, bool> HandleDestructuringPredicate { get; set; }
    public Func<PropertyInfo, bool>[] IgnoredPropertyPredicates { get; set; }
    public Type ExceptionType { get; set; }
}