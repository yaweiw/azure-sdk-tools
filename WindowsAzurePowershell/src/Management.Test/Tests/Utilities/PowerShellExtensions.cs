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

namespace Microsoft.WindowsAzure.Management.Test.Tests.Utilities
{
    using System.Management.Automation;

    public static class PowerShellExtensions
    {
        /// <summary>
        /// Gets a powershell varibale from the current session and convernts it back to it's original type.
        /// </summary>
        /// <typeparam name="T">The powershell object original type</typeparam>
        /// <param name="powershell">The PowerShell instance</param>
        /// <param name="name">The variable name</param>
        /// <returns>The variable object</returns>
        public static T GetPowerShellVariable<T>(this PowerShell powershell, string name)
        {
            object obj = powershell.Runspace.SessionStateProxy.GetVariable(name);

            if (obj is PSObject)
            {
                return (T)(obj as PSObject).BaseObject;
            }
            else
            {
                return (T)obj;
            }
        }
    }
}
