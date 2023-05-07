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
    /// Adds the Destructure.ByIgnoringProperties() extension to <see cref="LoggerDestructuringConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationIgnoreExtensions
    {
        /// <summary>
        /// Destructure.ByIgnoringProperties takes one or more expressions that access a property, e.g. obj => obj.Property, and uses the property names to determine which
        /// properties are ignored when an object of type TDestruture is destructured by serilog.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="ignoredProperty">The function expressions that expose the properties to ignore.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByIgnoringProperties<TDestructure>(this LoggerDestructuringConfiguration configuration, params Expression<Func<TDestructure, object>>[] ignoredProperty) =>
            configuration.ByIgnoringPropertiesWhere(obj => obj.GetType() == typeof(TDestructure), ignoredProperty);

        /// <summary>
        /// Destructure.ByIgnoringProperties takes one or more expressions that access a property, e.g. obj => obj.Property, and uses the property names to determine which
        /// properties are ignored when an object of type assignable to TDestructure is destructured by serilog.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="ignoredProperty">The function expressions that expose the properties to ignore.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByIgnoringPropertiesOfTypeAssignableTo<TDestructure>(this LoggerDestructuringConfiguration configuration, params Expression<Func<TDestructure, object>>[] ignoredProperty) =>
            configuration.ByIgnoringPropertiesWhere(obj => obj is TDestructure, ignoredProperty);

        /// <summary>
        /// Destructure.ByIgnoringProperties takes one or more expressions that access a property, e.g. obj => obj.Property, and uses the property names to determine which
        /// properties are ignored when an object for which destructureFunc returns true is destructured by serilog.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="handleDestructuringPredicate">Given an object to destructure, should this policy take effect?</param>
        /// <param name="ignoredProperty">The function expressions that expose the properties to ignore.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByIgnoringPropertiesWhere<TDestruture>(this LoggerDestructuringConfiguration configuration, Func<object, bool> handleDestructuringPredicate, params Expression<Func<TDestruture, object>>[] ignoredProperty)
        {
            return configuration.ByIgnoringPropertiesWhere(
                handleDestructuringPredicate,
                ignoredProperty
                    .Select(x => x.GetPropertyNameFromExpression())
                    .Select<string, Func<PropertyInfo, bool>>(ignoredPropertyName => pi => pi.Name == ignoredPropertyName)
                    .ToArray());
        }

        /// <summary>
        /// Destructure.ByIgnoringProperties takes one or more ignoredProperty predicates that when true indicates a given property is to be ignored when destructured by serilog.
        /// This ignoring only comes into play for an object where destructurePredicate returns true.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="handleDestructuringPredicate">Given an object to destructure, should this policy take effect?</param>
        /// <param name="ignoredPropertyPredicates">When the predicate returns true for a provided property, said will be ignored when destructured by serilog.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration ByIgnoringPropertiesWhere(this LoggerDestructuringConfiguration configuration, Func<object, bool> handleDestructuringPredicate, params Func<PropertyInfo, bool>[] ignoredPropertyPredicates) =>
            configuration.With(new DestructureByIgnoringPolicy(handleDestructuringPredicate, ignoredPropertyPredicates));
    }
}
