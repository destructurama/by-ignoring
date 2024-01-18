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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Destructurama.ByIgnoring
{
    internal sealed class DestructureByIgnoringPolicy : IDestructuringPolicy
    {
        private readonly Func<object, bool> _handleDestructuringPredicate;
        private readonly Func<PropertyInfo, bool>[] _ignoredPropertyPredicates;

        private readonly ConcurrentDictionary<Type, PropertyInfo[]> _cache = new();

        public DestructureByIgnoringPolicy(Func<object, bool> handleDestructuringPredicate, params Func<PropertyInfo, bool>[] ignoredPropertyPredicates)
        {
            _handleDestructuringPredicate = handleDestructuringPredicate ?? throw new ArgumentNullException(nameof(handleDestructuringPredicate));
            _ignoredPropertyPredicates = ignoredPropertyPredicates ?? throw new ArgumentNullException(nameof(ignoredPropertyPredicates));

            if (!ignoredPropertyPredicates.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(ignoredPropertyPredicates), "at least one ignore rule must be supplied");
            }
        }

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue? result)
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

        private PropertyInfo[] GetIncludedProperties(Type type)
        {
            var eligibleRuntimeProperties = type.GetRuntimeProperties()
                .Where(p => p.CanRead)
                .Where(p => p.GetMethod?.IsStatic != true)
                .Where(p => p.GetIndexParameters().Length == 0);

            return eligibleRuntimeProperties
                .Where(p => _ignoredPropertyPredicates.All(ignoreFunc => ignoreFunc(p) == false))
                .ToArray();
        }

        private static LogEventPropertyValue BuildStructure(object value, ILogEventPropertyValueFactory propertyValueFactory, IEnumerable<PropertyInfo> propertiesToInclude, Type destructureType)
        {
            var structureProperties = new List<LogEventProperty>();
            foreach (var propertyInfo in propertiesToInclude)
            {
                object propertyValue;
                try
                {
                    propertyValue = propertyInfo.GetValue(value);
                }
                catch (TargetInvocationException ex)
                {
                    SelfLog.WriteLine("The property accessor {0} threw exception {1}", propertyInfo, ex);
                    propertyValue = "The property accessor threw an exception: " + ex.InnerException.GetType().Name;
                }

                var logEventPropertyValue = BuildLogEventProperty(propertyValue, propertyValueFactory);

                structureProperties.Add(new LogEventProperty(propertyInfo.Name, logEventPropertyValue));
            }

            return new StructureValue(structureProperties, destructureType.Name);
        }

        private static LogEventPropertyValue BuildLogEventProperty(object propertyValue, ILogEventPropertyValueFactory propertyValueFactory)
        {
            return propertyValue == null ? new ScalarValue(null) : propertyValueFactory.CreatePropertyValue(propertyValue, destructureObjects: true);
        }
    }
}
