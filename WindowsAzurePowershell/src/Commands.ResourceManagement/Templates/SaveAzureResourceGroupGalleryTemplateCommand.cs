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

using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ResourceManagement.Templates
{
    /// <summary>
    /// Get one template or a list of templates from the gallery.
    /// </summary>
    [Cmdlet(VerbsData.Save, "AzureResourceGroupGalleryTemplate"), OutputType(typeof(bool))]
    public class SaveAzureResourceGroupGalleryTemplateCommand : ResourceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The gallery template name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The output path of the file.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "If specified, returns true after saving the file.")]
        public SwitchParameter PassThru { get; set; }

        public override void ExecuteCmdlet()
        {
            ResourceClient.DownloadGalleryTemplateFile(
                Name,
                string.IsNullOrEmpty(Path) ? System.IO.Path.Combine(CurrentPath(), Name) : this.TryResolvePath(Path));

            if (PassThru)
            {
                WriteObject(true);
            }
        }
    }
}
