using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests.TestCases;

public class ByIgnoringPropertiesOfTypeAssignableToTestCases
{
    public static IEnumerable<ByIgnoringTestCase<IDestructureMe>> IDestructureMeSuccessTestCases()
    {
        // TODO - I can't figure out a way to convert these from DestructureMeSuccessTestCases.ByIgnoringTestCases(), so we'll duplicate the scenarios here and change the type. If someone can figure this out please make them more like ByIgnoringWhereTestCases.cs
        yield return new ByIgnoringTestCase<IDestructureMe>("Ignore id and password should only include name")
        {
            IgnoredProperties = new Expression<Func<IDestructureMe, object>>[]
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

        yield return new ByIgnoringTestCase<IDestructureMe>("Ignore just id should include two others")
        {
            IgnoredProperties = new Expression<Func<IDestructureMe, object>>[]
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

        yield return new ByIgnoringTestCase<IDestructureMe>("Ignore just password should include two others")
        {
            IgnoredProperties = new Expression<Func<IDestructureMe, object>>[]
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

        yield return new ByIgnoringTestCase<IDestructureMe>("Ignoring all properties should produce empty object")
        {
            IgnoredProperties = new Expression<Func<IDestructureMe, object>>[]
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

        yield return new ByIgnoringTestCase<IDestructureMe>("Destructure policy shouldn't come into play for other types")
        {
            IgnoredProperties = new Expression<Func<IDestructureMe, object>>[]
            {
                dm => dm.Password, // reference type property
            },
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
        // TODO - I can't figure out a way to convert these from DestructureMeSuccessTestCases.ShouldThrowExceptionTestCases(), so we'll duplicate the scenarios here and change the type. If someone can figure this out please make them more like ByIgnoringWhereTestCases.cs
        yield return new ByIgnoreExceptionTestCase<IDestructureMe>("ComplexExpressionsFail")
        {
            IgnoredProperties = new Expression<Func<IDestructureMe, object>>[]
            {
                dm => new
                {
                    Name = dm.Name,
                }
            },
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<IDestructureMe>("MethodExpressionsFail")
        {
            IgnoredProperties = new Expression<Func<IDestructureMe, object>>[]
            {
                dm => dm.ToString(),
            },
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<IDestructureMe>("StringLiteralExpressionsFail")
        {
            IgnoredProperties = new Expression<Func<IDestructureMe, object>>[]
            {
                dm => "string literal",
            },
            ExceptionType = typeof(ArgumentException),
        };

        yield return new ByIgnoreExceptionTestCase<IDestructureMe>("ChainedPropertyExpressionsFail")
        {
            IgnoredProperties = new Expression<Func<IDestructureMe, object>>[]
            {
                dm => dm.Password.Length
            },
            ExceptionType = typeof(ArgumentException),
        };
    }
}