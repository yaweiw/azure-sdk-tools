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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MS.Test.Common.MsTestLib;

namespace Commands.Storage.ScenarioTest.BVT.HTTP
{
    using Common;
    using Util;

    /// <summary>
    /// bvt cases for anonymous storage account
    /// </summary>
    [TestClass]
    class AnonymousBVT : HTTPS.AnonymousBVT
    {
        [ClassInitialize()]
        public static void AnonymousHTTPBVTClassInitialize(TestContext testContext)
        {
            TestBase.TestClassInitialize(testContext);
            CLICommonBVT.SaveAndCleanSubScriptionAndEnvConnectionString();
            StorageAccountName = Test.Data.Get("StorageAccountName");
            StorageEndPoint = Test.Data.Get("StorageEndPoint").Trim();
            useHttps = false;
            PowerShellAgent.SetAnonymousStorageContext(StorageAccountName, useHttps, StorageEndPoint);
            downloadDirRoot = Test.Data.Get("DownloadDir");
            SetupDownloadDir();
        }

        [ClassCleanup()]
        public static void AnonymousHTTPBVTClassCleanup()
        {
            FileUtil.CleanDirectory(downloadDirRoot);
            TestBase.TestClassCleanup();
        }
    }
}
