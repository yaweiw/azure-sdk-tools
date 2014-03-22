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

using Microsoft.Azure.Commands.ResourceManager.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.Azure.Commands.ResourceManager
{
    public abstract class ResourceManagerBaseCmdlet : CmdletWithSubscriptionBase
    {
        private ResourcesClient resourcesClient;

        private GalleryTemplatesClient galleryTemplatesClient;

        public ResourcesClient ResourcesClient
        {
            get
            {
                if (resourcesClient == null)
                {
                    resourcesClient = new ResourcesClient(CurrentSubscription)
                    {
                        VerboseLogger = WriteVerboseWithTimestamp,
                        ErrorLogger = WriteErrorWithTimestamp
                    };
                }
                return resourcesClient;
            }

            set { resourcesClient = value; }
        }

        public GalleryTemplatesClient GalleryTemplatesClient
        {
            get
            {
                if (galleryTemplatesClient == null)
                {
                    galleryTemplatesClient = new GalleryTemplatesClient(CurrentSubscription);
                }
                return galleryTemplatesClient;
            }

            set { galleryTemplatesClient = value; }
        }
    }
}
