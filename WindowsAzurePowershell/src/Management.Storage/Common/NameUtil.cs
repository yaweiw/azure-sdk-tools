// ----------------------------------------------------------------------------------
//
// Copyright 2012 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Name utility
    /// </summary>
    internal class NameUtil
    {
        /// <summary>
        /// is valid container name <see cref="http://msdn.microsoft.com/en-us/library/windowsazure/dd135715.aspx"/>
        /// </summary>
        /// <param name="containerName">container name</param>
        /// <returns>true for valid container name, otherwise return false</returns>
        public static bool IsValidContainerName(string containerName)
        {
            Regex regex = new Regex(@"^\$root$|^\$logs$|^[a-z0-9]([a-z0-9]|(?<=[a-z0-9])-(?=[a-z0-9])){2,62}$");
            return regex.IsMatch(containerName);
        }

        /// <summary>
        /// is valid container prefix or not
        /// </summary>
        /// <param name="containerPrefix">container prefix</param>
        /// <returns>true for valid container prefix, otherwise return false</returns>
        public static bool IsValidContainerPrefix(string containerPrefix)
        {
            if (containerPrefix.Length > 0 && containerPrefix.Length < 3)
            {
                containerPrefix = containerPrefix + "abc";
            };

            return IsValidContainerName(containerPrefix);
        }

        /// <summary>
        /// is valid blob name <see cref="http://msdn.microsoft.com/en-us/library/windowsazure/dd135715.aspx"/>
        /// </summary>
        /// <param name="blobName">blob name</param>
        /// <returns>true for valid blob name, otherwise return false</returns>
        public static bool IsValidBlobName(string blobName)
        {
            Regex regex = new Regex(@"^[\s\S]{1,1024}$");
            return regex.IsMatch(blobName);
        }

        
        /// <summary>
        /// is valid table name <see cref="http://msdn.microsoft.com/en-us/library/windowsazure/dd179338.aspx"/>
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns>true for valid table name, otherwise return false</returns>
        public static bool IsValidTableName(string tableName)
        {
            Regex regex = new Regex(@"^[A-Za-z][A-Za-z0-9]{2,62}$");
            return regex.IsMatch(tableName);
        }

        /// <summary>
        /// is valid table prefix
        /// </summary>
        /// <param name="tablePrefix">table prefix</param>
        /// <returns>true for valid table prefix, otherwise return false</returns>
        public static bool IsValidTablePrefix(string tablePrefix)
        {
            if (tablePrefix.Length > 0 && tablePrefix.Length < 3)
            {
                tablePrefix = tablePrefix + "abc";
            };

            return IsValidTableName(tablePrefix);
        }

        
        /// <summary>
        /// is valid queue name <see cref="http://msdn.microsoft.com/en-us/library/windowsazure/dd179349.aspx"/>
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <returns>true for valid queue name, otherwise return false</returns>
        public static bool IsValidQueueName(string queueName)
        {
            Regex regex = new Regex(@"^[0-9a-z][a-z0-9-]{1,61}[0-9a-z]$");
            return regex.IsMatch(queueName);
        }

        /// <summary>
        /// is valid queue prefix
        /// </summary>
        /// <param name="queuePrefix">queue prefix</param>
        /// <returns>true for valid queue prefix, otherwise return false</returns>
        public static bool IsValidQueuePrefix(string queuePrefix)
        {
            if (queuePrefix.Length > 0 && queuePrefix.Length < 3)
            {
                queuePrefix = queuePrefix + "abc";
            };

            return IsValidQueueName(queuePrefix);
        }

        /// <summary>
        /// is valid file name in local machine
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <returns></returns>
        public static bool IsValidFileName(string fileName)
        {
            int maxFileLength = 255;

            if (string.IsNullOrEmpty(fileName) || fileName.Length > maxFileLength)
            {
                return false;
            }
            else if (fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1)
            {
                return false;
            }
            else
            {
                string realName = Path.GetFileNameWithoutExtension(fileName);
                //"CLOCK$", "COM0" are reserved
                string[] forbiddenList = { "CON", "PRN", "AUX", "NUL", 
                    "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                    "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
                bool forbidden = forbiddenList.Any(item => item == realName);
                return !forbidden;
            }
        }
    }
}
