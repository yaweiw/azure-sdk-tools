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

namespace Microsoft.WindowsAzure.Management.CloudService.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.CloudService.Model;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema;
    using VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Various utilities and helpers to facilitate testing.
    /// </summary>
    /// <remarks>
    /// The name is a compromise for something that pops up easily in
    /// intellisense when using MSTest.
    /// </remarks>
    public static class Testing
    {
        /// <summary>
        /// Validate a collection of assertions against files that are expected
        /// to exist in the file system watched by a FileSystemHelper.
        /// </summary>
        /// <param name="files">
        /// The FileSystemHelper watching the files.
        /// </param>
        /// <param name="assertions">
        /// Mapping of relative path names to actions that will validate the
        /// contents of the path.  Each action takes a full path to the file
        /// so it can be opened, verified, etc.  Null actions are allowed and
        /// serve to verify only that a file exists.
        /// </param>
        public static void AssertFiles(this FileSystemHelper files, Dictionary<string, Action<string>> assertions)
        {
            Assert.IsNotNull(files);
            Assert.IsNotNull(assertions);

            foreach (KeyValuePair<string, Action<string>> pair in assertions)
            {
                string path = files.GetFullPath(pair.Key);
                bool exists = File.Exists(path);
                Assert.IsTrue(exists, "Expected the existence of file {0}", pair.Key);
                if (exists && pair.Value != null)
                {
                    pair.Value(path);
                }
            }
        }

        /// <summary>
        /// Gets worker role object from service definition.
        /// </summary>
        /// <param name="rootPath">The azure service rootPath path</param>
        /// <returns>The worker role object</returns>
        internal static WorkerRole GetWorkerRole(string rootPath, string name)
        {
            AzureService service = new AzureService(rootPath, null);
            return service.Components.GetWorkerRole(name);
        }

        /// <summary>
        /// Gets web role object from service definition.
        /// </summary>
        /// <param name="rootPath">The azure service rootPath path</param>
        /// <returns>The web role object</returns>
        internal static WebRole GetWebRole(string rootPath, string name)
        {
            AzureService service = new AzureService(rootPath, null);
            return service.Components.GetWebRole(name);
        }

        /// <summary>
        /// Gets the role settings object from cloud service configuration.
        /// </summary>
        /// <param name="rootPath">The azure service rootPath path</param>
        /// <returns>The role settings object</returns>
        internal static RoleSettings GetCloudRole(string rootPath, string name)
        {
            AzureService service = new AzureService(rootPath, null);
            return service.Components.GetCloudConfigRole(name);
        }

        /// <summary>
        /// Gets the role settings object from local service configuration.
        /// </summary>
        /// <param name="rootPath">The azure service rootPath path</param>
        /// <returns>The role settings object</returns>
        internal static RoleSettings GetLocalRole(string rootPath, string name)
        {
            AzureService service = new AzureService(rootPath, null);
            return service.Components.GetLocalConfigRole(name);
        }
    }
}
