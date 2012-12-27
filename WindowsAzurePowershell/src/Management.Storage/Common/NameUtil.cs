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
        /// is valid container name
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// http://msdn.microsoft.com/en-us/library/windowsazure/dd135715.aspx
        public static bool IsValidContainerName(string s)
        {
            Regex regex = new Regex(@"^\$root$|^\$logs$|^[a-z0-9]([a-z0-9]|(?<=[a-z0-9])-(?=[a-z0-9])){2,62}$");
            return regex.IsMatch(s);
        }

        /// <summary>
        /// is valid container prefix or not
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsValidContainerPrefix(string s)
        {
            if (s.Length > 0 && s.Length < 3)
            {
                s = s + "abc";
            };
            return IsValidContainerName(s);
        }

        /// <summary>
        /// is valid blob name
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// http://msdn.microsoft.com/en-us/library/windowsazure/dd135715.aspx
        public static bool IsValidBlobName(string s)
        {
            Regex regex = new Regex(@"^[\s\S]{1,1024}$");
            return regex.IsMatch(s);
        }

        
        /// <summary>
        /// is valid table name
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// http://msdn.microsoft.com/en-us/library/windowsazure/dd179338.aspx
        public static bool IsValidTableName(string s)
        {
            Regex regex = new Regex(@"^[A-Za-z][A-Za-z0-9]{2,62}$");
            return regex.IsMatch(s);
        }

        /// <summary>
        /// is valid table prefix
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsValidTablePrefix(string s)
        {
            if (s.Length > 0 && s.Length < 3)
            {
                s = s + "abc";
            };
            return IsValidTableName(s);
        }

        
        /// <summary>
        /// is valid queue name
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// http://msdn.microsoft.com/en-us/library/windowsazure/dd179349.aspx
        public static bool IsValidQueueName(string s)
        {
            Regex regex = new Regex(@"^[0-9a-z][a-z0-9-]{1,61}[0-9a-z]$");
            return regex.IsMatch(s);
        }

        /// <summary>
        /// is valid queue prefix
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsValidQueuePrefix(string s)
        {
            if (s.Length > 0 && s.Length < 3)
            {
                s = s + "abc";
            };
            return IsValidQueueName(s);
        }

        /// <summary>
        /// is valid file name in local machine
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsValidFileName(string s)
        {
            bool valid = !string.IsNullOrEmpty(s) && s.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1;
            if (valid)
            {
                string fileName = Path.GetFileNameWithoutExtension(s);
                string[] forbiddenList = { "CON", "PRN", "AUX", "CLOCK$", "NUL", 
                    "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                    "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
                bool forbidden = forbiddenList.Any(item => item == fileName);
                valid = !forbidden;
            }
            return valid;
        }
    }
}
