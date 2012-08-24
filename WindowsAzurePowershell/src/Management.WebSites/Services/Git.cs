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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class Git
    {
        public static string GetConfigurationValue(string name)
        {
            return ExecuteGitProcess(string.Format("config --get {0}", name)).Split('\n').FirstOrDefault();
        }

        public static void SetConfigurationValue(string name, string value)
        {
            ExecuteGitProcess(string.Format("config {0} {1}", name, value));
        }

        public static void ClearConfigurationValue(string name)
        {
            ExecuteGitProcess(string.Format("config --unset {0}", name));
        }

        public static IList<string> GetRemoteRepositories()
        {
            return ExecuteGitProcess("remote").Split('\n');
        }

        public static void AddRemoteRepository(string name, string url)
        {
            ExecuteGitProcess(string.Format("remote add {0} {1}", name, url));
        }

        public static void RemoveRemoteRepository(string name)
        {
            ExecuteGitProcess(string.Format("remote rm {0}", name));
        }

        public static void InitRepository()
        {
            ExecuteGitProcess("init");
        }

        public static IList<string> GetWorkingTree()
        {
            return ExecuteGitProcess("rev-parse --git-dir").Split('\n');
        }

        public static string GetUri(string repositoryUri, string siteName, string auth)
        {
            UriBuilder uriBuilder = new UriBuilder(repositoryUri)
            {
                Path = siteName + ".git",
                UserName = auth
            };

            return uriBuilder.Uri.ToString();
        }

        private static string ExecuteGitProcess(string arguments)
        {
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = arguments;
                process.Start();

                // Read the output stream first and then wait.
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }
    }
}
