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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
	using System;
	using System.Globalization;
	using System.Management.Automation;

	public class VirtualMachineDscExtensionCmdletBase : VirtualMachineExtensionCmdletBase
    {
	    protected const string VirtualMachineDscExtensionCmdletNoun = "AzureVMDscExtension";
		protected const string ExtensionPublishedNamespace = "Microsoft.DSCExtension.Test"; // BUGBUG: Need to update Name and Namespace with production values
		protected const string ExtensionPublishedName = "DSC5.5";
        // This constant also used in Publish cmdlet, which is not inhereted from VirtualMachineDscExtensionCmdletBase.
        public const string DefaultContainerName = "windows-powershell-dsc";
        protected const string DefaultExtensionVersion = "1.*";

        public VirtualMachineDscExtensionCmdletBase()
        {
            this.extensionName = ExtensionPublishedName;
            this.publisherName = ExtensionPublishedNamespace;
        }

		protected void ThrowArgumentError(string format, params object[] args)
		{
			ThrowTerminatingError(
				new ErrorRecord(
					new ArgumentException(string.Format(CultureInfo.CurrentUICulture, format, args)),
					string.Empty,
					ErrorCategory.InvalidArgument,
					null));
		}
	}
}
