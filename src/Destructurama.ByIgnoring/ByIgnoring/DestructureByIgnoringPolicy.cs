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

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Destructurama.ByIgnoring;

internal sealed class DestructureByIgnoringPolicy : IDestructuringPolicy
{
    private readonly Func<object, bool> _handleDestructuringPredicate;
    private readonly Func<PropertyInfo, bool>[] _ignoredPropertyPredicates;
    private readonly Func<PropertyInfo, object, bool> _ignoreValuePredicate;

    private readonly ConcurrentDictionary<Type, (PropertyInfo Property, Func<object, object> Accessor)[]> _cache = new();

    public DestructureByIgnoringPolicy(Func<object, bool> handleDestructuringPredicate, Func<PropertyInfo, object, bool> ignoreValuePredicate, params Func<PropertyInfo, bool>[] ignoredPropertyPredicates)
    {
        _handleDestructuringPredicate = handleDestructuringPredicate ?? throw new ArgumentNullException(nameof(handleDestructuringPredicate));
        _ignoreValuePredicate = ignoreValuePredicate;
        _ignoredPropertyPredicates = ignoredPropertyPredicates ?? throw new ArgumentNullException(nameof(ignoredPropertyPredicates));

        // After introducing ignoreValuePredicate caller now may not specify any value for ignoredPropertyPredicates.
        // if (ignoredPropertyPredicates.Length == 0)
        //     throw new ArgumentOutOfRangeException(nameof(ignoredPropertyPredicates), "At least one ignore rule must be supplied");
    }

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        if (value == null || !_handleDestructuringPredicate(value))
        {
            result = null;
            return false;
        }

        var type = value.GetType();
        var includedProperties = _cache.GetOrAdd(type, GetIncludedProperties);

        result = BuildStructure(value, propertyValueFactory, includedProperties, type);

        return true;
    }

    private (PropertyInfo Property, Func<object, object> Accessor)[] GetIncludedProperties(Type type)
    {
        var eligibleRuntimeProperties = type.GetRuntimeProperties()
            .Where(p => p.CanRead)
            .Where(p => !p.GetMethod.IsStatic)
            .Where(p => p.GetIndexParameters().Length == 0);

        return eligibleRuntimeProperties
            .Where(p => _ignoredPropertyPredicates.All(ignoreFunc => !ignoreFunc(p)))
            .Select(p => (p, Compile(p)))
            .ToArray();

        static Func<object, object> Compile(PropertyInfo property)
        {
            var objParameterExpr = Expression.Parameter(typeof(object), "instance");
            var instanceExpr = Expression.Convert(objParameterExpr, property.DeclaringType);
            var propertyExpr = Expression.Property(instanceExpr, property);
            var propertyObjExpr = Expression.Convert(propertyExpr, typeof(object));
            return Expression.Lambda<Func<object, object>>(propertyObjExpr, objParameterExpr).Compile();
        }
    }

    private StructureValue BuildStructure(object value, ILogEventPropertyValueFactory propertyValueFactory, (PropertyInfo Property, Func<object, object> Accessor)[] propertiesToInclude, Type destructureType)
    {
        var structureProperties = new List<LogEventProperty>();
        foreach (var (propertyInfo, accessor) in propertiesToInclude)
        {
            object propertyValue;
            bool ignoreValue = false;
            try
            {
                propertyValue = accessor(value);
                ignoreValue = _ignoreValuePredicate(propertyInfo, propertyValue);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("The property accessor {0} threw exception {1}", propertyInfo, ex);
                propertyValue = "The property accessor threw an exception: " + ex.GetType().Name;
            }

            if (!ignoreValue)
            {
                var logEventPropertyValue = BuildLogEventProperty(propertyValue, propertyValueFactory);

                structureProperties.Add(new LogEventProperty(propertyInfo.Name, logEventPropertyValue));
            }
        }

        return new StructureValue(structureProperties, destructureType.Name);
    }

    private static LogEventPropertyValue BuildLogEventProperty(object propertyValue, ILogEventPropertyValueFactory propertyValueFactory)
        => propertyValue == null
            ? ScalarValue.Null
            : propertyValueFactory.CreatePropertyValue(propertyValue, destructureObjects: true);
}
