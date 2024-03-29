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

using FluentAssertions;
using FluentAssertions.Equivalency;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests;

public static class FluentAssertionExtensions
{
    public static EquivalencyAssertionOptions<T> UsingSerilogTypeComparisons<T>(this EquivalencyAssertionOptions<T> options)
    {
        return options
            .Using<ScalarValue>(ctx => ctx.Subject.Value.Should().BeEquivalentTo(ctx.Expectation.Value))
            .WhenTypeIs<ScalarValue>();
    }
}
