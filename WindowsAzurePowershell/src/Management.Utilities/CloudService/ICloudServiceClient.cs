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

namespace Microsoft.WindowsAzure.Management.Utilities.CloudService
{
    using Microsoft.WindowsAzure.Management.Utilities.Common;

    public interface ICloudServiceClient
    {
        /// <summary>
        /// Starts a cloud service.
        /// </summary>
        /// <param name="name">The cloud service name</param>
        /// <param name="slot">The deployment slot</param>
        void StartCloudService(string name, string slot);

        /// <summary>
        /// Stops a cloud service.
        /// </summary>
        /// <param name="name">The cloud service name</param>
        /// <param name="slot">The deployment slot</param>
        void StopCloudService(string name, string slot);

        /// <summary>
        /// Check if the deployment exists for given cloud service.
        /// </summary>
        /// <param name="name">The cloud service name</param>
        /// <param name="slot">The deployment slot name</param>
        /// <returns>Flag indicating the deployment exists or not</returns>
        bool DeploymentExists(string name, string slot);
    }
}
