// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A utility class that will go through a series of properties,
    /// compare the values of the properties against a reference
    /// object, and if different will copy the source property to
    /// a destination object. Useful for set-azureX style cmdlets.
    /// </summary>
    public class ObjectDeltaMapper
    {
        private static readonly object[] noArgs = new object[0];
        private static readonly BindingFlags publicProperties = BindingFlags.Public | BindingFlags.Instance;

        public static bool Map<TSource, TReference, TDest>(TSource source, TReference reference, TDest dest, params string[] excludedProperties)
        {
            bool changed = false;
            var propertiesToCopy = GetPropertiesToUpdate<TSource, TReference, TDest>(excludedProperties);
            foreach (var property in propertiesToCopy)
            {
                changed = changed || UpdateProperty(property, source, reference, dest);
            }
            return changed;
        }

        private class PropertyNameComparer: IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                return string.Compare(x.Name, y.Name, StringComparison.InvariantCulture) == 0;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private static IEnumerable<PropertyInfo> GetPropertiesToUpdate<TSource, TReference, TDest>(string[] excludedProperties)
        {
            return PropertiesOf<TSource>().Where(pi => NotExcluded(pi, excludedProperties))
                .Intersect(PropertiesOf<TReference>(), new PropertyNameComparer())
                .Intersect(PropertiesOf<TDest>(), new PropertyNameComparer());
        }

        private static IEnumerable<PropertyInfo> PropertiesOf<T>()
        {
            return typeof (T).GetProperties(publicProperties);
        }

        private static bool NotExcluded(PropertyInfo property, string[] excludedProperties)
        {
            return excludedProperties.All(s => s != property.Name);
        }

        private static bool UpdateProperty<TSource, TReference, TDest>(PropertyInfo property, TSource source,
            TReference reference, TDest dest)
        {
            object sourceValue = GetSourceValue(property, source);
            if (sourceValue != null && !sourceValue.Equals(GetReferenceValue(property, reference)))
            {
                SetDestProperty(property, sourceValue, dest);
                return true;
            }
            return false;
        }

        private static object GetSourceValue(PropertyInfo property, object source)
        {
            return property.GetGetMethod().Invoke(source, noArgs);
        }

        private static PropertyInfo MatchingPropertyOf<T>(PropertyInfo sourceProperty)
        {
            return typeof (T).GetProperty(sourceProperty.Name, publicProperties);
        }

        private static object GetReferenceValue<TReference>(PropertyInfo property, TReference reference)
        {
            PropertyInfo referenceProperty = MatchingPropertyOf<TReference>(property);
            return referenceProperty.GetGetMethod().Invoke(reference, noArgs);
        }

        private static void SetDestProperty<TDest>(PropertyInfo sourceProperty, object sourceValue, TDest dest)
        {
            PropertyInfo destProperty = MatchingPropertyOf<TDest>(sourceProperty);
            destProperty.GetSetMethod().Invoke(dest, new[] {sourceValue});
        }
    }
}
