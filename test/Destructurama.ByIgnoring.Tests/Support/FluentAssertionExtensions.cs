// (C) Copyright 2023 Nullable, Inc. All rights reserved.
// This file is part of Nullable's product Aware and cannot be copied and/or
// distributed without the express permission of Nullable, Inc.

using FluentAssertions;
using FluentAssertions.Equivalency;
using Serilog.Events;

namespace Destructurama.ByIgnoring.Tests.Support;

public static class FluentAssertionExtensions
{
    public static EquivalencyAssertionOptions<T> UsingSerilogTypeComparisons<T>(this EquivalencyAssertionOptions<T> options)
    {
        return options
            .Using<ScalarValue>(ctx => ctx.Subject.Value.Should().BeEquivalentTo(ctx.Expectation.Value))
            .WhenTypeIs<ScalarValue>();
    }
}