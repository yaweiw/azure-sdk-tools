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

namespace Microsoft.WindowsAzure.Management.Utilities
{
    using System;
    using System.IO;
    using Properties;

    public static class PathUtility
    {
        public static string GetServicePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            // Get the service path
            var servicePath = FindServiceRootDirectory(path);
            
            // Was the service path found?
            if (servicePath == null)
            {
                throw new InvalidOperationException(Resources.CannotFindServiceRoot);
            }
            
            return servicePath;
        }

        public static string FindServiceRootDirectory(string path)
        {
            // Is the csdef file present in the folder
            bool found = Directory.GetFiles(path, Resources.ServiceDefinitionFileName).Length == 1;
            
            if (found)
            {
                return path; //return it
            }

            // Find the last slash
            int slash = path.LastIndexOf('\\');
            if (slash > 0)
            {
                // Slash found trim off the last path
                path = path.Substring(0, slash);
                
                // Recurse
                return FindServiceRootDirectory(path);
            }

            // Couldn't locate the service root, exit
            return null;
        }
    }
}