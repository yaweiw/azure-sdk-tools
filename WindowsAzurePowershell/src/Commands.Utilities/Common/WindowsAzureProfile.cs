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
    using Properties;

    /// <summary>
    /// This class is the entry point for all the persistent
    /// state related to azure connections, including
    /// current environment, subscriptions, etc.
    /// </summary>
    public class WindowsAzureProfile
    {
        // Internal state

        // Store - responsible for loading and saving a profile to a particular location
        private IProfileStore profileStore;

        // Azure environments
        private readonly Dictionary<string, WindowsAzureEnvironment> environments = new Dictionary<string, WindowsAzureEnvironment>(
            WindowsAzureEnvironment.PublicEnvironments, StringComparer.OrdinalIgnoreCase);

        // Singleton instance management
        private static readonly Lazy<WindowsAzureProfile> instance =
            new Lazy<WindowsAzureProfile>(() => new WindowsAzureProfile());

        public static WindowsAzureProfile Instance
        {
            get
            {
                return instance.Value;
            }
        }
        
        //
        // Azure environments
        //

        public IDictionary<string, WindowsAzureEnvironment> Environments { 
            get { return new Dictionary<string, WindowsAzureEnvironment>(environments); }
        }

        private WindowsAzureEnvironment currentEnvironment = null;

        public WindowsAzureEnvironment CurrentEnvironment
        {
            get
            {
                if (currentEnvironment == null)
                {
                    currentEnvironment = environments[EnvironmentName.AzureCloud];
                }
                return currentEnvironment;
            }

            set
            {
                if (!environments.ContainsKey(value.Name))
                {
                    AddEnvironment(value);
                }
                currentEnvironment = environments[value.Name];

                Save();
            }
        }

        public void AddEnvironment(WindowsAzureEnvironment newEnvironment)
        {
            if (environments.ContainsKey(newEnvironment.Name))
            {
                throw new InvalidOperationException(string.Format(Resources.EnvironmentExists, newEnvironment.Name));
            }

            environments[newEnvironment.Name] = newEnvironment;

            Save();
        }

    }
}