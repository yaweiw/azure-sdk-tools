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
    //This class bridge the concept GAP between "memeory size" at front end 
    //and "sku count" at back end
    class CacheSkuCountConvert
    {
        private string offeringName;   
        public CacheSkuCountConvert(string offering)
        {
            offeringName = offering;
        }

        public int ToCount (string memorySize)
        {
            return 1;
        }

        public string ToMemorySize (int skuCount)
        {
            return "123";
        }
    }
}
