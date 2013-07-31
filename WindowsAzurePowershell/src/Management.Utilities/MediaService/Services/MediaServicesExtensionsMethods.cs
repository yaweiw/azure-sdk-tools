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


using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services.MediaServicesEntities;

namespace Microsoft.WindowsAzure.Management.Utilities.MediaService.Services
{
    public static class MediaServicesExtensionsMethods
    {
        public static MediaServiceAccounts GetMediaServices(this IMediaServiceManagement proxy, string subscriptionName)
        {
            return proxy.EndGetMediaServices(proxy.BeginGetMediaServices(subscriptionName, null, null));
        }

        public static MediaServiceAccountDetails GetMediaService(this IMediaServiceManagement proxy, string subscriptionName, string name)
        {
            return proxy.EndGetMediaService(proxy.BeginGetMediaService(subscriptionName, name, null, null));
        }

        public static void DeleteMediaServicesAccount(this IMediaServiceManagement proxy, string subscriptionName, string accountName)
        {
            proxy.EndDeleteMediaServicesAccount(proxy.BeginDeleteMediaServicesAccount(subscriptionName, accountName, null, null));
        }

        public static void RegenerateMediaServicesAccount(this IMediaServiceManagement proxy, string subscriptionName, string accountName, string keyType)
        {
            proxy.EndRegenerateMediaServicesAccount(proxy.BeginRegenerateMediaServicesAccount(subscriptionName, accountName, keyType, null, null));
        }
    }
}