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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MS.Test.Common.MsTestLib;
using StorageTestLib;

namespace CLITest.Util
{
    class FileUtil
    {
        /// <summary>
        /// generate temp files using StorageTestLib helper
        /// </summary>
        /// <param name="rootPath">the root dir path</param>
        /// <param name="relativePath">the relative dir path</param>
        /// <param name="depth">sub dir depth</param>
        /// <param name="files">a list of created files</param>
        private static void GenerateTempFiles(string rootPath, string relativePath, int depth, List<string> files)
        {
            Random random = new Random();
            string[] specialNames = { "pageabc", "blockabc ", "pagea b", "block abc", "page??", "block? ?", "page ??", "block?? ", "page.abc", "block.a bc", "page .abc", "block .abc ", string.Empty };

            //TODO minEntityCount should be 0 after using parallel uploading and downloading. refer to bug#685185
            int minEntityCount = 1;
            int maxEntityCount = 5;

            int fileCount = random.Next(minEntityCount, maxEntityCount);
            int nameCount = specialNames.Count() - 1;

            for (int i = 0; i < fileCount; i++)
            {
                int specialIndex = random.Next(0, nameCount);
                string prefix = specialNames[specialIndex];
                string fileName = Path.Combine(relativePath, Utility.GenNameString(prefix));
                int fileSize = random.Next(1, nameCount);
                string filePath = Path.Combine(rootPath, fileName);
                files.Add(fileName);
                Helper.GenerateRandomTestFile(filePath, fileSize);
                Test.Info("Create a {0}kb test file '{1}'", fileSize, filePath);
            }

            int dirCount = random.Next(minEntityCount, maxEntityCount);
            for (int i = 0; i < dirCount; i++)
            {
                int specialIndex = random.Next(0, nameCount);
                string prefix = specialNames[specialIndex];
                string dirName = Path.Combine(relativePath, Utility.GenNameString(string.Format("dir{0}", prefix)));
                //TODO dir name should contain space
                dirName = dirName.Replace(" ", "");
                string absolutePath = Path.Combine(rootPath, dirName);
                Directory.CreateDirectory(absolutePath);
                Test.Info("Create directory '{0}'", absolutePath);

                if (depth >= 1)
                {
                    GenerateTempFiles(rootPath, dirName, depth - 1, files);
                }
            }
        }

        /// <summary>
        /// create temp dirs and files
        /// </summary>
        /// <param name="rootPath">the destination dir</param>
        /// <param name="depth">sub dir depth</param>
        /// <returns>a list of created files</returns>
        public static List<string> GenerateTempFiles(string rootPath, int depth)
        {
            List<string> files = new List<string>();
            files.Clear();
            GenerateTempFiles(rootPath, string.Empty, depth, files);
            files.Sort();
            return files;
        }

        /// <summary>
        /// clean the specified dir
        /// </summary>
        /// <param name="directory">the destination dir</param>
        public static void CleanDirectory(string directory)
        {
            DirectoryInfo dir = new DirectoryInfo(directory);

            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                CleanDirectory(subdir.FullName);
                subdir.Delete();
            }

            Test.Info("Clean directory {0}", directory);
        }
    }
}
