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
using Microsoft.WindowsAzure.Common.OData;
using System;
using System.Collections.Generic;
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
        public virtual Uri GetGalleryTemplateFile(string templateName)
        {
            return new Uri(GalleryClient.Items.Get(templateName).Item.DefinitionTemplates.DeploymentTemplateFileUrls.First().Value);
        }

        /// <summary>
        /// Filters gallery templates based on the passed options.
        /// </summary>
        /// <param name="options">The filter options</param>
        /// <returns>The filtered list</returns>
        public virtual List<GalleryItem> FilterGalleryTemplates(FilterGalleryTemplatesOptions options)
        {
            StringBuilder filterString = new StringBuilder();

            if (!string.IsNullOrEmpty(options.Publisher))
            {
                filterString.Append(FilterString.Generate<ItemListFilter>(f => f.Publisher == options.Publisher));
            }

            // To Do: fix the FilterString to generate valid query for this code
            //filterString.Append(FilterString.Generate<ItemListFilter>(f => 
            //    f.Publisher == options.Publisher &&
            //    f.CategoryIds.Contains(options.Category) &&
            //    f.Name == options.Name));

            ItemListResult filtered = GalleryClient.Items.List(new ItemListParameters()
            {
                Filter = filterString.ToString()
            });

            return filtered.Items.ToList();
        }
    }
}