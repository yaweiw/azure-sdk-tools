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
    public class PowerShellTest
    {
        public static string ErrorIsNotEmptyException = "Test failed due to a non-empty error stream, check the error stream in the test log for more details";

        protected PowerShell powershell;
        protected string[] modules;

        public PowerShellTest(params string[] modules)
        {
            this.modules = modules;
        }

        protected void AddScenarioScript(string script)
        {
            powershell.AddScript(Testing.GetTestResourceContents(script));
        }

        public virtual Collection<PSObject> RunPowerShellTest(params string[] scripts)
        {
            Collection<PSObject> output = null;
            for (int i = 0; i < scripts.Length; ++i)
            {
                Console.WriteLine(scripts[i]);
                powershell.AddScript(scripts[i]);
            }
            try
            {
                output = powershell.Invoke();
                if (powershell.HadErrors || powershell.Streams.Error.Count > 0)
                {
                    throw new RuntimeException(ErrorIsNotEmptyException);
                }

                return output;
            }
            catch (Exception psException)
            {
                powershell.LogPowerShellException(psException);
                throw;
            }
            finally
            {
                Console.WriteLine("History: {0}", powershell.HistoryString);
                powershell.LogPowerShellResults(output);
            }
        }

        [TestInitialize]
        public virtual void SetupTest()
        {
            powershell = PowerShell.Create();

            foreach (string moduleName in modules)
            {
                powershell.AddScript(string.Format("Import-Module \"{0}\"", Testing.GetTestResourcePath(moduleName)));
            }

            powershell.AddScript("$VerbosePreference='Continue'");
            powershell.AddScript("$DebugPreference='Continue'");
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            powershell.Dispose();
        }
    }
}
