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

namespace Microsoft.WindowsAzure.Management.Websites.Services
{
    using Microsoft.WindowsAzure.Management.Websites.Services.Github;
    using Microsoft.WindowsAzure.Management.Websites.Services.WebEntities;
    using System;
    using System.IO;
    using System.Linq;

    public abstract class LinkedRevisionControl
    {
        protected string invocationPath;
        public abstract void Init();
        public abstract void Deploy(Site siteData);

        internal bool IsGitWorkingTree()
        {
            return Git.GetWorkingTree().Any(line => line.Equals(".git"));
        }

        internal void InitGitOnCurrentDirectory()
        {
            Git.InitRepository();

            if (!File.Exists(".gitignore"))
            {
                // Scaffold gitignore
                string cmdletPath = Directory.GetParent(invocationPath).FullName;
                File.Copy(Path.Combine(cmdletPath, "Resources/Scaffolding/Node/.gitignore"), ".gitignore");
            }
        }
    }
}
