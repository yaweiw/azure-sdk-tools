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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Utilities
{
    using System.Management.Automation;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using System.Collections.ObjectModel;
    using System;

    [TestClass]
    public class PowerShellAzureTest : PowerShellTest
    {
        protected TestCredentialHelper credentials;
        protected string credentialFile;

        public PowerShellAzureTest(params string[] modules)
            : base(modules)
        {
            this.credentials = new TestCredentialHelper(Environment.CurrentDirectory);
            this.credentialFile = TestCredentialHelper.DefaultCredentialFile;
        }

        [TestInitialize]
        public override void SetupTest()
        {
            base.SetupTest();
            this.credentials.SetupPowerShellEnvironment(powershell, this.credentialFile);
        }
    }
}
