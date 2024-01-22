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
        var log1 = new LoggerConfiguration()
            .Destructure.ByIgnoringProperties<DestructureMe>(o => o.Id)
            .CreateLogger();

        var processor1 = log1.GetType().GetField("_messageTemplateProcessor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(log1)!;
        var converter1 = processor1.GetType().GetField("_propertyValueConverter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(processor1)!;
        _factory1 = (ILogEventPropertyValueFactory)converter1;
        var policies1 = (IDestructuringPolicy[])converter1.GetType().GetField("_destructuringPolicies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(converter1)!;
        _policy1 = policies1.First(p => p is DestructureByIgnoringPolicy);

        var log2 = new LoggerConfiguration()
            .Destructure.ByIgnoringPropertiesOfTypeAssignableTo<IDestructureMe>(o => o.Id)
            .CreateLogger();

        var processor2 = log2.GetType().GetField("_messageTemplateProcessor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(log2)!;
        var converter2 = processor2.GetType().GetField("_propertyValueConverter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(processor2)!;
        _factory2 = (ILogEventPropertyValueFactory)converter2;
        var policies2 = (IDestructuringPolicy[])converter2.GetType().GetField("_destructuringPolicies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(converter2)!;
        _policy2 = policies2.First(p => p is DestructureByIgnoringPolicy);
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
