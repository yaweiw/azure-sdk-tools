// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.Utilities.Common
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class WindowsAzureEnvironment
    {
        /// <summary>
        /// The Windows Azure environment name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The management portal endpoint.
        /// </summary>
        public string PortalEndpoint { get; set; }

        public static Dictionary<string, WindowsAzureEnvironment> PublicEnvironments
        {
            get { return environments; }
            private set;
        }

        private static Dictionary<string, WindowsAzureEnvironment> environments = 
            new Dictionary<string, WindowsAzureEnvironment>()
        {
            {
                EnvironmentName.Azure,
                new WindowsAzureEnvironment()
                {
                    Name = EnvironmentName.Azure,
                    PortalEndpoint = EnvironmentPortalEndpoint.Azure
                }
            },
            {
                EnvironmentName.China,
                new WindowsAzureEnvironment()
                {
                    Name = EnvironmentName.China,
                    PortalEndpoint = EnvironmentPortalEndpoint.China
                }
            }
        };
    }

    class EnvironmentName
    {
        public const string Azure = "Azure";

        public const string China = "China";
    }

    class EnvironmentPortalEndpoint
    {
        public const string Azure = "https://manage.windowsazure.com/publishsettings/index/";

        public const string China = "https://manage.windowsazure.cn/publishsettings/index/";
    }
}
