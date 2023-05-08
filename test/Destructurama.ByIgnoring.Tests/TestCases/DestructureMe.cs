// (C) Copyright 2023 Nullable, Inc. All rights reserved.
// This file is part of Nullable's product Aware and cannot be copied and/or
// distributed without the express permission of Nullable, Inc.

namespace Destructurama.ByIgnoring.Tests.TestCases;

public class DestructureMe : IDestructureMe
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public static string SomeStatic { get; set; } = "AAA";
    public string this[int index] => "value";
}