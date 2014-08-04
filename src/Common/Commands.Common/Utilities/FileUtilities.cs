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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using Commands.Common.Properties;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public static class FileUtilities
    {
        public static string GetAssemblyDirectory()
        {
            var assemblyPath = Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
            return Path.GetDirectoryName(assemblyPath);
        }

        public static string GetContentFilePath(string fileName)
        {
            return GetContentFilePath(GetAssemblyDirectory(), fileName);
        }

        public static string GetContentFilePath(string startDirectory, string fileName)
        {
            string path = Path.Combine(startDirectory, fileName);

            // Try search in the subdirectories in case that the file path does not exist in root path
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                try
                {
                    path = Directory.GetDirectories(startDirectory, fileName, SearchOption.AllDirectories).FirstOrDefault();

                    if (string.IsNullOrEmpty(path))
                    {
                        path = Directory.GetFiles(startDirectory, fileName, SearchOption.AllDirectories).First();
                    }
                }
                catch
                {
                    throw new FileNotFoundException(Path.Combine(startDirectory, fileName));
                }
            }

            return path;
        }

        public static string GetWithProgramFilesPath(string directoryName, bool throwIfNotFound)
        {
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (Directory.Exists(Path.Combine(programFilesPath, directoryName)))
            {
                return Path.Combine(programFilesPath, directoryName);
            }
            else
            {
                if (programFilesPath.IndexOf(Resources.x86InProgramFiles) == -1)
                {
                    programFilesPath += Resources.x86InProgramFiles;
                    if (throwIfNotFound)
                    {
                        Validate.ValidateDirectoryExists(Path.Combine(programFilesPath, directoryName));
                    }
                    return Path.Combine(programFilesPath, directoryName);
                }
                else
                {
                    programFilesPath = programFilesPath.Replace(Resources.x86InProgramFiles, String.Empty);
                    if (throwIfNotFound)
                    {
                        Validate.ValidateDirectoryExists(Path.Combine(programFilesPath, directoryName));
                    }
                    return Path.Combine(programFilesPath, directoryName);
                }
            }
        }

        public static void CreateDirectories(IEnumerable<string> directories)
        {
            foreach (string directoryName in directories)
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        public static void CopyFilesFromResources(IDictionary<string, string> filesPair)
        {
            foreach (KeyValuePair<string, string> filePair in filesPair)
            {
                File.WriteAllBytes(filePair.Value, GeneralUtilities.GetResourceContents(filePair.Key));
            }
        }

        /// <summary>
        /// Write all of the given bytes to a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="mode">Mode to open the file.</param>
        /// <param name="bytes">Contents of the file.</param>
        public static void WriteAllBytes(string path, FileMode mode, byte[] bytes)
        {
            Debug.Assert(!String.IsNullOrEmpty(path));
            Debug.Assert(Enum.IsDefined(typeof(FileMode), mode));
            Debug.Assert(bytes != null && bytes.Length > 0);

            // Note: We're not wrapping the file in a using statement because
            // that could lead to a double dispose when the writer is disposed.
            FileStream file = null;
            try
            {
                file = new FileStream(path, mode);
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    // Clear the reference to file so it won't get disposed twice
                    file = null;

                    writer.Write(bytes);
                }
            }
            finally
            {
                if (file != null)
                {
                    file.Dispose();
                }
            }
        }

        /// <summary>
        /// Copies a directory.
        /// </summary>
        /// <param name="sourceDirName">The source directory name</param>
        /// <param name="destDirName">The destination directory name</param>
        /// <param name="copySubDirs">Should the copy be recursive</param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(String.Format(Resources.PathDoesNotExist, sourceDirName));
            }

            Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// Ensures that a directory exists beofre attempting to write a file
        /// </summary>
        /// <param name="pathName">The path to the file that will be created</param>
        public static void EnsureDirectoryExists(string pathName)
        {
            Validate.ValidateStringIsNullOrEmpty(pathName, "Settings directory");
            string directoryPath = Path.GetDirectoryName(pathName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Create a unique temp directory.
        /// </summary>
        /// <returns>Path to the temp directory.</returns>
        public static string CreateTempDirectory()
        {
            string tempPath = null;
            do
            {
                tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
            while (Directory.Exists(tempPath) || File.Exists(tempPath));

            Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        /// <summary>
        /// Copy a directory from one path to another.
        /// </summary>
        /// <param name="sourceDirectory">Source directory.</param>
        /// <param name="destinationDirectory">Destination directory.</param>
        public static void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            Debug.Assert(!String.IsNullOrEmpty(sourceDirectory), "sourceDictory cannot be null or empty!");
            Debug.Assert(Directory.Exists(sourceDirectory), "sourceDirectory must exist!");
            Debug.Assert(!String.IsNullOrEmpty(destinationDirectory), "destinationDirectory cannot be null or empty!");
            Debug.Assert(!Directory.Exists(destinationDirectory), "destinationDirectory must not exist!");

            foreach (string file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Substring(
                    sourceDirectory.Length + 1,
                    file.Length - sourceDirectory.Length - 1);
                string destinationPath = Path.Combine(destinationDirectory, relativePath);

                string destinationDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                File.Copy(file, destinationPath);
            }
        }

        public static Encoding GetFileEncoding(string path)
        {
            Encoding encoding;


            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path, true))
                {
                    encoding = r.CurrentEncoding;
                }
            }
            else
            {
                encoding = Encoding.Default;
            }

            return encoding;
        }

        public static string CombinePath(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// Returns true if path is a valid directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidDirectoryPath(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return false;
            }

            try
            {
                FileAttributes attributes = File.GetAttributes(path);

                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void RecreateDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// Gets the root installation path for the given Azure module.
        /// </summary>
        /// <param name="module" >The module name</param>
        /// <returns>The module full path</returns>
        public static string GetPSModulePathForModule(AzureModule module)
        {
            return GetContentFilePath(GetInstallPath(), GetModuleFolderName(module));
        }

        /// <summary>
        /// Gets the root directory for all modules installation.
        /// </summary>
        /// <returns>The install path</returns>
        public static string GetInstallPath()
        {
            string currentPath = GetAssemblyDirectory();
            while (!currentPath.EndsWith(GetModuleFolderName(AzureModule.AzureProfile)) &&
                   !currentPath.EndsWith(GetModuleFolderName(AzureModule.AzureResourceManager)) &&
                   !currentPath.EndsWith(GetModuleFolderName(AzureModule.AzureServiceManagement)))
            {
                currentPath = Directory.GetParent(currentPath).FullName;
            }

            // The assemption is that the install directory looks like that:
            // ServiceManagement
            //  AzureServiceManagement
            //      <Service Commands Folders>
            // ResourceManager
            //  AzureResourceManager
            //      <Service Commands Folders>
            // Profile
            //  AzureProfile
            //      <Service Commands Folders>
            return Directory.GetParent(currentPath).FullName;
        }

        public static string GetModuleName(AzureModule module)
        {
            switch (module)
            {
                case AzureModule.AzureServiceManagement:
                    return "Azure";

                case AzureModule.AzureResourceManager:
                    return "AzureResourceManager";

                case AzureModule.AzureProfile:
                    return "AzureProfile";

                default:
                    throw new ArgumentOutOfRangeException(module.ToString());
            }
        }

        private static string GetModuleFolderName(AzureModule module)
        {
            return module.ToString().Replace("Azure", "");
        }
    }
}
