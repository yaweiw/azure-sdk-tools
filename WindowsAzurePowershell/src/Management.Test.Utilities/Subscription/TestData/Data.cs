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

namespace Microsoft.WindowsAzure.Management.Test.TestData
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Tests.Utilities;
    using WindowsAzure.Management.Utilities.Properties;

    public static class Data
    {
        public static string Subscription1 = "Windows Azure Sandbox 9-220";
        public static string SampleSubscription1 = "Sample Subscription 1";

        public static List<string> ValidServiceName { get; private set; }
        public static List<string> ValidPublishSettings { get; private set; }
        public static List<string> ValidPublishSettings2 { get; private set; }
        public static List<string> ValidSubscriptionsData { get; private set; }
        public static List<string> ValidSubscriptionName { get; private set; }
        public static List<string> ValidServiceRootName { get; private set; }

        public static List<string> InvalidSubscriptionsData { get; private set; } 
        public static List<string> InvalidPublishSettings { get; private set; }
        public static List<string> InvalidServiceRootName { get; private set; }
        public static List<string> InvalidFileName { get; private set; }

        public static string AzureAppDir { get; private set; }

        static Data()
        {
            AzureAppDir = Path.Combine(Directory.GetCurrentDirectory(), Resources.AzureDirectoryName);

            ValidServiceName = new List<string>();
            InitializeValidServiceNameData();

            ValidPublishSettings = new List<string>();
            InitializeValidPublishSettingsData();

            ValidPublishSettings2 = new List<string>();
            InitializeValidPublishSettings2Data();

            ValidSubscriptionsData = new List<string>();
            InitializeValidSubscriptionsData();

            ValidSubscriptionName = new List<string>();
            InitializeValidSubscriptionNameData();

            ValidServiceRootName = new List<string>();
            InitializeValidServiceRootNameData();

            InvalidSubscriptionsData = new List<string>();
            InitializeInvalidSubscriptionsData();

            InvalidPublishSettings = new List<string>();
            InitializeInvalidPublishSettingsData();

            InvalidServiceRootName = new List<string>();
            InitializeInvalidServiceRootNameData();

            InvalidFileName = new List<string>();
            InitializeInvalidFileNameData();
        }

        private static void InitializeValidServiceNameData()
        {
            ValidServiceName.Add("HelloNode");
            ValidServiceName.Add("node.jsservice");
            ValidServiceName.Add("node_js_service");
            ValidServiceName.Add("node-js-service");
            ValidServiceName.Add("node-js-service123");
            ValidServiceName.Add("123node-js-service123");
            ValidServiceName.Add("123node-js2service");
        }

        private static void InitializeInvalidPublishSettingsData()
        {
            InvalidPublishSettings.Add(Testing.GetTestResourcePath("InvalidProfile.PublishSettings"));
        }

        private static void InitializeValidPublishSettingsData()
        {
            ValidPublishSettings.Add(Testing.GetTestResourcePath("ValidProfile.PublishSettings"));
        }

        private static void InitializeValidPublishSettings2Data()
        {
            ValidPublishSettings2.Add(Testing.GetTestResourcePath("ValidProfile2.PublishSettings"));
        }

        private static void InitializeValidSubscriptionsData()
        {
            ValidSubscriptionsData.Add(Testing.GetTestResourcePath("subscriptions.xml"));
        }

        private static void InitializeInvalidSubscriptionsData()
        {
            InvalidSubscriptionsData.Add(Testing.GetTestResourcePath("invalidsubscriptions.xml"));
        }

        private static void InitializeValidSubscriptionNameData()
        {
            ValidSubscriptionName.Add("mysub1");
            ValidSubscriptionName.Add("mysub2");
            ValidSubscriptionName.Add(Data.Subscription1);
            ValidSubscriptionName.Add("TestSubscription1");
            ValidSubscriptionName.Add("TestSubscription2");
        }

        /// <summary>
        /// This method must run after InitializeServiceRootNameData()
        /// </summary>
        private static void InitializeInvalidServiceRootNameData()
        {
            char[] invalidPathNameChars = System.IO.Path.GetInvalidPathChars();

            for (int i = 0, j = 0; i < invalidPathNameChars.Length; i++)
            {
                StringBuilder invalidPath = new StringBuilder(ValidServiceRootName[j]);
                invalidPath[invalidPath.Length / 2] = invalidPathNameChars[i];
                j %= ValidServiceRootName.Count;
                InvalidServiceRootName.Add(invalidPath.ToString());
            }
        }

        private static void InitializeValidServiceRootNameData()
        {
            ValidServiceRootName.AddRange(ValidServiceName);
        }

        private static void InitializeInvalidFileNameData()
        {
            char[] invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();

            // Validations that depend on Path.GetFileName fails with these characters. For example:
            // if user entered name for WebRole as "My/WebRole", then Path.GetFileName get file name as WebRole.
            //
            char[] ignoreSet = { ':', '\\', '/' };

            for (int i = 0, j = 0; i < invalidFileNameChars.Length; i++, j++)
            {
                if (ignoreSet.Contains<char>(invalidFileNameChars[i]))
                {
                    continue;
                }
                j %= ValidServiceRootName.Count - 1;
                StringBuilder invalidFile = new StringBuilder(ValidServiceRootName[j]);
                invalidFile[invalidFile.Length / 2] = invalidFileNameChars[i];
                InvalidFileName.Add(invalidFile.ToString());
            }
        }
    }
}