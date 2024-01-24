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
using Destructurama.ByIgnoring;
using Serilog;
using Serilog.Core;

namespace Benchmarks;

public class ByIgnoringBenchmarks
{
    private readonly DestructureMe _obj = new()
    {
        Id = 42,
        Name = "Tom",
        Password = "12345",
    };

    private ILogEventPropertyValueFactory _factory1 = null!;
    private ILogEventPropertyValueFactory _factory2 = null!;
    private IDestructuringPolicy _policy1 = null!;
    private IDestructuringPolicy _policy2 = null!;

    [GlobalSetup]
    public void Setup()
    {
        (_policy1, _factory1) = Build(c => c.Destructure.ByIgnoringProperties<DestructureMe>(o => o.Id));
        (_policy2, _factory2) = Build(c => c.Destructure.ByIgnoringPropertiesOfTypeAssignableTo<DestructureMe>(o => o.Id));
    }

    private static (IDestructuringPolicy, ILogEventPropertyValueFactory) Build(Func<LoggerConfiguration, LoggerConfiguration> configure)
    {
        var configuration = new LoggerConfiguration();
        var logger = configure(configuration).CreateLogger();

        var processor = logger.GetType().GetField("_messageTemplateProcessor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(logger)!;
        var converter = processor.GetType().GetField("_propertyValueConverter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(processor)!;
        var factory = (ILogEventPropertyValueFactory)converter;
        var policies = (IDestructuringPolicy[])converter.GetType().GetField("_destructuringPolicies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(converter)!;
        var policy = policies.First(p => p is DestructureByIgnoringPolicy);
        return (policy, factory);
    }

    [Benchmark]
    public void Destructure()
    {
        _policy1.TryDestructure(_obj, _factory1, out _);
    }

    [Benchmark]
    public void DestructureAssignable()
    {
        _policy2.TryDestructure(_obj, _factory2, out _);
    }
}
