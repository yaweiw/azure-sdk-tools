// ----------------------------------------------------------------------------------
//
// Copyright 2012 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ---------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    public class StorageBlobBaseCmdlet : StorageBaseCmdlet
    {
        internal IBlobManagement blobClient = null;
        //auto clean blob client in order to work with multiple storage account
        private bool autoClean = false;

        protected override void ProcessRecord()
        {
            if (blobClient == null)
            {
                autoClean = true;
                blobClient = new BlobManagement(GetCloudBlobClient());
            }

            base.ProcessRecord();

            if (autoClean)
            {
                blobClient = null;
                autoClean = false;
            }
        }
    }
}