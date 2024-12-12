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

using System.Linq.Expressions;
using System.Reflection;
using Destructurama.ByIgnoring;

namespace Destructurama;

/// <summary>
/// Options for family of extension methods of <see cref="LoggerConfigurationIgnoreExtensions"/>.
/// </summary>
/// <typeparam name="TDestructure">Type of object to be destructured by Serilog.</typeparam>
public sealed class IgnoreOptions<TDestructure>
{
    /// <summary>
    /// Given an object to destructure, should this policy take effect?
    /// </summary>
    internal Func<object, bool> HandleDestructuringPredicate { get; set; } = obj => obj.GetType() == typeof(TDestructure);

    /// <summary>
    /// When the predicate returns true for a provided property, said will be ignored when destructured by Serilog.
    /// </summary>
    internal List<Func<PropertyInfo, bool>> IgnoredPropertyPredicates { get; set; } = new();

    /// <summary>
    /// Predicate to ignore properties with specific values.
    /// </summary>
    internal Func<PropertyInfo, object, bool> IgnoreValuePredicate { get; set; } = (_, _) => false;

    /// <summary>
    /// Policy takes effect when object's type is <typeparamref name="TDestructure"/>.
    /// </summary>
    /// <returns>Reference to itself.</returns>
    public IgnoreOptions<TDestructure> DestructureExactType()
    {
        HandleDestructuringPredicate = obj => obj is TDestructure;
        return this;
    }

    /// <summary>
    /// Policy takes effect when object of type assignable to <typeparamref name="TDestructure"/>.
    /// </summary>
    /// <returns>Reference to itself.</returns>
    public IgnoreOptions<TDestructure> DestructureAssignableTo()
    {
        HandleDestructuringPredicate = obj => obj is TDestructure;
        return this;
    }

    /// <summary>
    /// Takes expression that access a property, e.g. obj => obj.Property,
    /// and uses the property name to determine which property is ignored.
    /// </summary>
    /// <param name="ignoredProperty">The function expression that expose the property to ignore.</param>
    /// <returns>Reference to itself.</returns>
    public IgnoreOptions<TDestructure> Ignore(Expression<Func<TDestructure, object?>> ignoredProperty)
    {
        var name = ignoredProperty.GetPropertyNameFromExpression();
        IgnoredPropertyPredicates.Add(pi => pi.Name == name);
        return this;
    }

    /// <summary>
    /// Takes predicate to ignore properties with specific values.
    /// For example, it allows you to ignore all properties that return null or empty strings:
    /// <code>
    /// IgnoreValue((_, v) => v is null || v is string s &amp;&amp; s.Length == 0);
    /// </code>
    /// </summary>
    /// <param name="ignoreValuePredicate">Predicate to ignore properties with specific values.</param>
    /// <returns>Reference to itself.</returns>
    public IgnoreOptions<TDestructure> IgnoreValue(Func<PropertyInfo, object, bool> ignoreValuePredicate)
    {
        IgnoreValuePredicate = ignoreValuePredicate;
        return this;
    }
}

