// (C) Copyright 2023 Nullable, Inc. All rights reserved.
// This file is part of Nullable's product Aware and cannot be copied and/or
// distributed without the express permission of Nullable, Inc.

using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests.TestCases;

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

public class ByIgnoreWhereTestCases
{
    public static IEnumerable<ByIgnoreWhereTestCase> ShouldDestructureSuccessfullyTestCases()
    {
        yield return new ByIgnoreWhereTestCase("Ignore id and password should only include name")
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
}