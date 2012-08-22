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

namespace Microsoft.WindowsAzure.Management.WebSites.Services
{
    using System.Diagnostics;

    public class GitWebSite
    {
        public string Name { get; set; }
        public string Webspace { get; set; }

        public GitWebSite(string name, string webspace)
        {
            Name = name;
            Webspace = webspace;
        }

        public void WriteConfiguration()
        {
            SetGitConfigurationValue("azure.site.name", Name);
            SetGitConfigurationValue("azure.site.webspace", Webspace);
        }

        public static GitWebSite ReadConfiguration()
        {
            return new GitWebSite(
                GetGitConfigurationValue("azure.site.name"),
                GetGitConfigurationValue("azure.site.webspace"));
        }

        private static string GetGitConfigurationValue(string name)
        {
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = string.Format("config --get {0}", name);
                process.Start();

                // Read the output stream first and then wait.
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split('\n');
                
            }

            return null;
        }

        private static void SetGitConfigurationValue(string name, string value)
        {
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = string.Format("config {0} {1}", name, value);
                process.Start();

                // Read the output stream first and then wait.
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
        }
    }
}
