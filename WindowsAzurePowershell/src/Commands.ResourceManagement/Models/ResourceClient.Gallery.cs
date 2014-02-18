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

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Microsoft.Azure.Commands.ResourceManagement.Properties;
using Microsoft.Azure.Commands.ResourceManagement.ResourceGroups;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Azure.Management.Resources;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Newtonsoft.Json;
using System.Linq;
using System.Management.Automation;
using Microsoft.CSharp.RuntimeBinder;
using System.Diagnostics;
using System.Security;
using System.Threading;
using Microsoft.Azure.Gallery;
using Microsoft.Azure.Gallery.Models;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public partial class ResourcesClient
    {
        public virtual Uri GetGallaryTemplateFile(string templateName)
        {
            return new Uri(GalleryClient.Items.Get(templateName).Item.DefinitionTemplates.DeploymentTemplateFileUrls.First().Value);
        }
    }
}
