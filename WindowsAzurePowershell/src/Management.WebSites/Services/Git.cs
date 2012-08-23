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
    using System.Collections.Generic;
    using System.Diagnostics;

    public abstract class Git
    {
        public static string GetConfigurationValue(string name)
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

                return output;
            }
        }

        public static void SetConfigurationValue(string name, string value)
        {
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = string.Format("config {0} {1}", name, value);
                process.Start();
                process.WaitForExit();
            }
        }

        public static IList<string> GetRemoteRepos()
        {
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = "remote";
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Split('\n');
            }
        } 

        public static void InitRepository()
        {
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = "init";
                process.Start();
                process.WaitForExit();
            }
        }

        public static IList<string> GetWorkingTree()
        {
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = "rev-parse --git-dir";
                process.Start();

                // Read the output stream first and then wait.
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Split('\n');
            }
        } 
    }
}
