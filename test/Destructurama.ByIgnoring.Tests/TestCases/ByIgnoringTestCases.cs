// (C) Copyright 2023 Nullable, Inc. All rights reserved.
// This file is part of Nullable's product Aware and cannot be copied and/or
// distributed without the express permission of Nullable, Inc.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests.TestCases;

public record ByIgnoringTestCase<TDestructure>(string TestName)
{
    public Expression<Func<TDestructure, object>>[] IgnoredProperties { get; set; }
    public object ObjectToDestructure { get; set; }
    public IDictionary<string, LogEventPropertyValue> ExpectedPropertiesLogged { get; set; }
    public Type MyType => typeof(TDestructure);
}

public record ByIgnoreExceptionTestCase<TDestructure>(string TestName)
{
    public Expression<Func<TDestructure, object>>[] IgnoredProperties { get; set; }
    public Type ExceptionType { get; set; }
}

public class ByIgnoringTestCases
{
    public static IEnumerable<ByIgnoringTestCase<DestructureMe>> DestructureMeSuccessTestCases()
    {
        yield return new ByIgnoringTestCase<DestructureMe>("Ignore id and password should only include name")
        {
            IgnoredProperties = new Expression<Func<DestructureMe, object>>[]
            {
                dm => dm.Id, // value type property
                dm => dm.Password, // reference type property
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

        yield return new ByIgnoringTestCase<DestructureMe>("Ignore just id should include two others")
        {
            IgnoredProperties = new Expression<Func<DestructureMe, object>>[]
            {
                dm => dm.Id, // value type property
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
                { "Password", new ScalarValue("Password") },
            },
        };

        yield return new ByIgnoringTestCase<DestructureMe>("Ignore just password should include two others")
        {
            IgnoredProperties = new Expression<Func<DestructureMe, object>>[]
            {
                dm => dm.Password, // reference type property
            },
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
            IgnoredProperties = new Expression<Func<DestructureMe, object>>[]
            {
                dm => dm.Password,
                dm => dm.Name,
                dm => dm.Id,
            },
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
            IgnoredProperties = new Expression<Func<DestructureMe, object>>[]
            {
                dm => dm.Password, // reference type property
            },
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
            IgnoredProperties = new Expression<Func<DestructureMeWithPropertyWithOnlySetter, object>>[]
            {
                dm => dm.Id, // value type property
                dm => dm.Password, // reference type property
            },
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
                { "Id", new ScalarValue("2") },
                { "Name", new ScalarValue("CoolName") },
                { "Password", new ScalarValue("Password") },
            },
        };
    }

    public static IEnumerable<ByIgnoreExceptionTestCase<DestructureMe>> ShouldThrowExceptionTestCases()
    {
        yield return new ByIgnoreExceptionTestCase<DestructureMe>("ComplexExpressionsFail")
        {
            IgnoredProperties = new Expression<Func<DestructureMe, object>>[]
            {
                dm => new
                {
                    Name = dm.Name,
                }
            },
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<DestructureMe>("MethodExpressionsFail")
        {
            IgnoredProperties = new Expression<Func<DestructureMe, object>>[]
            {
                dm => dm.ToString(),
            },
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<DestructureMe>("StringLiteralExpressionsFail")
        {
            IgnoredProperties = new Expression<Func<DestructureMe, object>>[]
            {
                dm => "string literal",
            },
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<DestructureMe>("ChainedPropertyExpressionsFail")
        {
            IgnoredProperties = new Expression<Func<DestructureMe, object>>[]
            {
                dm => dm.Password.Length
            },
            ExceptionType = typeof(ArgumentException),
        };
    }

    public class DestructureMeWithPropertyWithOnlySetter
    {
        private string _onlySetter;
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string OnlySetter { set { _onlySetter = value; } }
    }

    public class SomeOtherType
    {
        public string FullName { get; set; }
    }
}