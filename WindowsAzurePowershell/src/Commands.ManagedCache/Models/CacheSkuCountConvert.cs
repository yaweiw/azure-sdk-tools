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

namespace Microsoft.Azure.Commands.ManagedCache.Models
{
    using System;

    //This class bridge the concept gap between "memeory size" at front end 
    //and "sku count" at back end
    class CacheSkuCountConvert
    {
        private string skuName;
        private int min;
        private int max;
        private int increment;
        private string unit; //MB or GB
        public CacheSkuCountConvert(string sku)
        {
            skuName = sku;
            if (IsBasicSku(sku))
            {
                min = 128;
                max = 1024;
                increment = 128;
                unit = "MB";
            }
            else if (IsStandardSku(sku))
            {
                min = 1;
                max = 10;
                increment = 1;
                unit = "GB";
            }
            else if (IsPremiumSku(sku))
            {
                min = 5;
                max = 150;
                increment = 5;
                unit = "GB";
            }
            else
            {
                throw new ArgumentException(Properties.Resources.InvalidCacheSku);
            }
        }

        public int ToSkuCount (string memorySize)
        {
            if (string.IsNullOrEmpty(memorySize))
            {
                return 1;
            }
            if (memorySize.EndsWith(unit))
            {
                memorySize = memorySize.Substring(0, memorySize.Length - unit.Length);
            }
            int size;
            if (!int.TryParse(memorySize, out size) ||
                size < min || size > max)
            {
                throw new ArgumentException(
                    string.Format(Properties.Resources.InvalidCacheMemorySize, min, max, unit));
            }
            return  size / increment;
        }

        public string ToMemorySize (int skuCount)
        {
            return string.Format("{0}{1}", skuCount * increment, unit);
        }

        private bool IsBasicSku(string sku)
        {
            return string.IsNullOrEmpty(sku) || sku.Equals("Basic", System.StringComparison.OrdinalIgnoreCase);
        }

        private bool IsStandardSku(string sku)
        {
            return sku.Equals("Standard", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPremiumSku(string sku)
        {
            return sku.Equals("Premium", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
