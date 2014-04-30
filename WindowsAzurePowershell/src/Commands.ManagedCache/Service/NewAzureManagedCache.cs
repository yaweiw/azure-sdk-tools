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

namespace Microsoft.Azure.Commands.ManagedCache
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.ManagedCache;
    using System.Security.Cryptography;
    using System.Text;

    [Cmdlet(VerbsCommon.New, "AzureManagedCache", ConfirmImpact = ConfirmImpact.None)]
    public class NewAzureManagedCache : ManagedCacheCmdletBase
    {
        private string cloudServiceName;
        private string cacheServiceName;

        [Parameter(Position = 0, 
            HelpMessage = "azure cache service name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Position = 1, 
            Mandatory = true, 
            HelpMessage = "The geographic region to create the website.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(Position = 2,
            Mandatory = false)]
        [ValidateSet("Basic", "Standard", "Premium", IgnoreCase = true)]
        public string Sku
        {
            get;
            set;
        }

        [Parameter(Position = 3,
            Mandatory = false,
            HelpMessage = "The cache memeory size")]
        public string Memory
        {
            get;
            set;
        }

        public override void ExecuteCmdlet()
        {
            //TODO, validate the Name, length much be between 6~20;
            //Only lower case letter and number, and start with letter
            EnsureCloudService();

            cacheServiceName = Name.ToLower();
            CloudServiceGetResponse.Resource newCacheService = CreateCacheServices();
            WriteObject(newCacheService);
        }

        private void EnsureCloudService()
        {
            cloudServiceName = GetCloudServiceName(CurrentSubscription.SubscriptionId, Location);
            if (!CloudServiceExists())
            {
                CloudServiceCreateParameters parameters = new CloudServiceCreateParameters();
                parameters.GeoRegion = Location;
                parameters.Description = cloudServiceName;
                parameters.Label = cloudServiceName;
                OperationResponse response = CacheClient.CloudServices.Create(cloudServiceName, parameters);
            }
        }

        public CloudServiceGetResponse.Resource CreateCacheServices()
        {
            CacheServiceCreateParameters param = new CacheServiceCreateParameters();
            IntrinsicSettings settings = new IntrinsicSettings();
            IntrinsicSettings.CacheServiceInput input = new IntrinsicSettings.CacheServiceInput();
            settings.CacheServiceInputSection = input;
            param.Settings = settings;
            input.Location = Location;
            input.SkuCount = 1; //TODO
            input.ServiceVersion = "1.0.0";
            input.ObjectSizeInBytes = 1024;
            input.SkuType = Sku;

            CacheClient.CacheServices.CreateCacheService(cloudServiceName, cacheServiceName, param);

            CloudServiceGetResponse.Resource newCacheResource = null;
            //service state goes through Creating/Updating to Active. WE only care about active
            int waitInMinutes = 30; //minutes
            while (waitInMinutes > 0)
            {
                newCacheResource = GetCacheService();
                if (newCacheResource.SubState == CACHE_SERVICE_READY_STATE)
                {
                    break;
                }
                else
                {
                    //TODO: use PS fancy progress ?
                    System.Threading.Thread.Sleep(60000);
                    waitInMinutes--;
                }
            }

            if (waitInMinutes < 0)
            {
                throw new InvalidOperationException("Time out to wait for cache service ready");
            }
            return newCacheResource;
        }

        public CloudServiceGetResponse.Resource GetCacheService()
        {
            CloudServiceGetResponse resp = CacheClient.CloudServices.Get(cloudServiceName);
            CloudServiceGetResponse.Resource cacheResource = null;
            foreach (var resource in resp.Resources)
            {
                if (resource.Type == CACHE_RESOURCE_TYPE && 
                    resource.Name.Equals(cacheServiceName, StringComparison.OrdinalIgnoreCase))
                {
                    cacheResource = resource;
                }
            }
            
            if (cacheResource == null)
            {
                //TODO: use resource strings
                throw new InvalidOperationException("Not able to find the cache service just now created");
            }
            return cacheResource;
        }

        public bool CloudServiceExists()
        {
            try
            {
                CloudServiceGetResponse response = CacheClient.CloudServices.Get(cloudServiceName);
            }
            catch(CloudException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw ex;
            }
            return true;
        }

        /// <summary>
        /// The following logic was ported from Azure Cache management portal. It is critical to maintain the 
        /// parity. Do not modify unless you understand the consequence.
        /// </summary>
        public string GetCloudServiceName(string subscriptionId, string region)
        {
            string hashedSubId = string.Empty;
            string extensionPrefix = CACHE_RESOURCE_TYPE;
            using (SHA256 sha256 = SHA256Managed.Create())
            {
                hashedSubId = Base32NoPaddingEncode(sha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(subscriptionId)));
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", extensionPrefix, hashedSubId, region.Replace(' ', '-'));
        }

        private string Base32NoPaddingEncode(byte[] data)
        {
            const string Base32StandardAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

            StringBuilder result = new StringBuilder(Math.Max((int)Math.Ceiling(data.Length * 8 / 5.0), 1));

            byte[] emptyBuffer = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] workingBuffer = new byte[8];

            // Process input 5 bytes at a time
            for (int i = 0; i < data.Length; i += 5)
            {
                int bytes = Math.Min(data.Length - i, 5);
                Array.Copy(emptyBuffer, workingBuffer, emptyBuffer.Length);
                Array.Copy(data, i, workingBuffer, workingBuffer.Length - (bytes + 1), bytes);
                Array.Reverse(workingBuffer);
                ulong val = BitConverter.ToUInt64(workingBuffer, 0);

                for (int bitOffset = ((bytes + 1) * 8) - 5; bitOffset > 3; bitOffset -= 5)
                {
                    result.Append(Base32StandardAlphabet[(int)((val >> bitOffset) & 0x1f)]);
                }
            }

            return result.ToString();
        } 
    }
}