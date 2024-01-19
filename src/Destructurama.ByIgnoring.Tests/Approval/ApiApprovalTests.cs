using NUnit.Framework;
using PublicApiGenerator;
using Shouldly;

namespace Destructurama.ByIgnoring.Tests;

/// <summary>Tests for checking changes to the public API.</summary>
[TestFixture]
public class ApiApprovalTests
{
    /// <summary>Check for changes to the public APIs.</summary>
    /// <param name="type">The type used as a marker for the assembly whose public API change you want to check.</param>
    [TestCase(typeof(LoggerConfigurationIgnoreExtensions))]
    public void PublicApi_Should_Not_Change_Unintentionally(Type type)
    {
        string publicApi = type.Assembly.GeneratePublicApi(new()
        {
            IncludeAssemblyAttributes = false,
            AllowNamespacePrefixes = ["System", "Microsoft.Extensions.DependencyInjection"],
            ExcludeAttributes = ["System.Diagnostics.DebuggerDisplayAttribute"],
        });

        publicApi.ShouldMatchApproved(options => options.NoDiff().WithFilenameGenerator((testMethodInfo, discriminator, fileType, fileExtension) => $"{type.Assembly.GetName().Name!}.{fileType}.{fileExtension}"));
    }
}
