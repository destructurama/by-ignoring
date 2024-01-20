using BenchmarkDotNet.Attributes;
using Destructurama;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Benchmarks;

public class ByIgnoringBenchmarks
{
    private readonly DestructureMe _obj = new()
    {
        Id = 42,
        Name = "Tom",
        Password = "12345",
    };

    private Logger _log1 = null!;
    private Logger _log2 = null!;

    [GlobalSetup]
    public void Setup()
    {
        LogEvent evt = null!;

        _log1 = new LoggerConfiguration()
            .Destructure.ByIgnoringProperties<DestructureMe>(o => o.Id)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        _log2 = new LoggerConfiguration()
            .Destructure.ByIgnoringPropertiesOfTypeAssignableTo<IDestructureMe>(o => o.Id)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();
    }

    [Benchmark]
    public void Log1()
    {
        _log1.Information("Here is {@Logged}", _obj);
    }

    [Benchmark]
    public void Log2()
    {
        _log2.Information("Here is {@Logged}", _obj);
    }
}
