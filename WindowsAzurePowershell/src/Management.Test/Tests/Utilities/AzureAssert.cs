// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.Test.Tests.Utilities
{
    using VisualStudio.TestTools.UnitTesting;
    using Management.Services;
    using XmlSchema;

    internal static class AzureAssert
    {
        public static void AreEqualGlobalPathInfo(GlobalPathInfo expected, GlobalPathInfo actual)
        {
            AreEqualGlobalPathInfo(expected.AzureDirectory, expected.PublishSettingsFile, actual);
        }

        public static void AreEqualGlobalPathInfo(string azureSdkPath, string publishSettings, GlobalPathInfo actual)
        {
            Assert.AreEqual(publishSettings, actual.PublishSettingsFile);
            Assert.AreEqual(azureSdkPath, actual.AzureDirectory);
        }

        public static void AreEqualGlobalComponents(GlobalComponents expected, GlobalComponents actual)
        {
            AreEqualGlobalComponents(expected.GlobalPaths, expected.PublishSettings, actual);
        }

        public static void AreEqualGlobalComponents(GlobalPathInfo paths, PublishData publishSettings, GlobalComponents actual)
        {
            AreEqualGlobalPathInfo(paths, actual.GlobalPaths);
        }
    }
}