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
