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

using Microsoft.Azure.Gallery;
using Microsoft.Azure.Gallery.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Common.OData;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public partial class ResourcesClient
    {
        /// <summary>
        /// Gets the uri of the specified template name.
        /// </summary>
        /// <param name="templateName">The fully qualified template name</param>
        /// <returns>The template uri</returns>
        public virtual string GetGalleryTemplateFile(string templateName)
        {
            return GalleryClient.Items.Get(templateName).Item.DefinitionTemplates.DeploymentTemplateFileUrls.First().Value;
        }

        /// <summary>
        /// Filters gallery templates based on the passed options.
        /// </summary>
        /// <param name="options">The filter options</param>
        /// <returns>The filtered list</returns>
        public virtual List<GalleryItem> FilterGalleryTemplates(FilterGalleryTemplatesOptions options)
        {
            List<string> filterStrings = new List<string>();
            ItemListParameters parameters = null;

            if (!string.IsNullOrEmpty(options.Publisher))
            {
                filterStrings.Add(FilterString.Generate<ItemListFilter>(f => f.Publisher == options.Publisher));
            }

            if (!string.IsNullOrEmpty(options.Category))
            {
                filterStrings.Add(FilterString.Generate<ItemListFilter>(f => f.CategoryIds.Contains(options.Category)));
            }

            if (!string.IsNullOrEmpty(options.Name))
            {
                filterStrings.Add(FilterString.Generate<ItemListFilter>(f => f.Name == options.Name));
            }

            if (filterStrings.Count > 0)
            {
                parameters = new ItemListParameters() { Filter = string.Join(" and ", filterStrings) };
            }

            return GalleryClient.Items.List(parameters).Items.ToList();
        }

        /// <summary>
        /// Downloads a gallery template file into specific directory.
        /// </summary>
        /// <param name="name">The gallery template file name</param>
        /// <param name="outputPath">The output file path</param>
        public virtual void DownloadGalleryTemplateFile(string name, string outputPath)
        {
            string fileUri = GetGalleryTemplateFile(name);
            StringBuilder finalOutputPath = new StringBuilder();
            string contents = GeneralUtilities.DownloadFile(fileUri);

            if (FileUtilities.IsValidDirectoryPath(outputPath))
            {
                finalOutputPath.Append(Path.Combine(outputPath, name + ".json"));
            }
            else
            {
                finalOutputPath.Append(outputPath);
                if (!Path.HasExtension(outputPath))
                {
                    finalOutputPath.Append(".json");
                }
            }

            File.WriteAllText(finalOutputPath.ToString(), contents);
        }
    }
}