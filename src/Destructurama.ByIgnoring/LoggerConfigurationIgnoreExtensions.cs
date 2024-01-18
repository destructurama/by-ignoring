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

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Destructurama.ByIgnoring;
using Serilog;
using Serilog.Configuration;

namespace Destructurama
{
    /// <summary>
    /// Adds the Destructure.ByIgnoringProperties() family of extension methods to <see cref="LoggerDestructuringConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationIgnoreExtensions
    {
        /// <summary>
        /// Takes one or more expressions that access a property, e.g. obj => obj.Property,
        /// and uses the property names to determine which properties are ignored when an
        /// object of type <typeparamref name="TDestructure"/> is destructured by Serilog.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="ignoredProperties">The function expressions that expose the properties to ignore.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByIgnoringProperties<TDestructure>(this LoggerDestructuringConfiguration configuration, params Expression<Func<TDestructure, object?>>[] ignoredProperties) =>
            configuration.ByIgnoringPropertiesWhere(obj => obj.GetType() == typeof(TDestructure), ignoredProperties);

        /// <summary>
        /// Takes one or more expressions that access a property, e.g. obj => obj.Property,
        /// and uses the property names to determine which properties are ignored when an
        /// object of type assignable to <typeparamref name="TDestructure"/> is destructured by Serilog.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="ignoredProperties">The function expressions that expose the properties to ignore.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByIgnoringPropertiesOfTypeAssignableTo<TDestructure>(this LoggerDestructuringConfiguration configuration, params Expression<Func<TDestructure, object?>>[] ignoredProperties) =>
            configuration.ByIgnoringPropertiesWhere(obj => obj is TDestructure, ignoredProperties);

        /// <summary>
        /// Takes one or more expressions that access a property, e.g. obj => obj.Property,
        /// and uses the property names to determine which properties are ignored when an
        /// object for which <paramref name="handleDestructuringPredicate"/> returns <see langword="true"/> is destructured by Serilog.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="handleDestructuringPredicate">Given an object to destructure, should this policy take effect?</param>
        /// <param name="ignoredProperties">The function expressions that expose the properties to ignore.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByIgnoringPropertiesWhere<TDestruture>(this LoggerDestructuringConfiguration configuration, Func<object, bool> handleDestructuringPredicate, params Expression<Func<TDestruture, object?>>[] ignoredProperties)
        {
            return configuration.ByIgnoringPropertiesWhere(
                handleDestructuringPredicate,
                ignoredProperties
                    .Select(x => x.GetPropertyNameFromExpression())
                    .Select<string, Func<PropertyInfo, bool>>(ignoredPropertyName => pi => pi.Name == ignoredPropertyName)
                    .ToArray());
        }

        /// <summary>
        /// Takes one or more predicates that when <see langword="true"/> indicates a given property is to
        /// be ignored when destructured by Serilog. This ignoring only comes into play for an object where
        /// <paramref name="handleDestructuringPredicate"/> returns true.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="handleDestructuringPredicate">Given an object to destructure, should this policy take effect?</param>
        /// <param name="ignoredPropertyPredicates">When the predicate returns true for a provided property, said will be ignored when destructured by Serilog.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByIgnoringPropertiesWhere(this LoggerDestructuringConfiguration configuration, Func<object, bool> handleDestructuringPredicate, params Func<PropertyInfo, bool>[] ignoredPropertyPredicates) =>
            configuration.With(new DestructureByIgnoringPolicy(handleDestructuringPredicate, ignoredPropertyPredicates));
    }
}
