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
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    class NameUtil
    {
        // The regular expression below is build up as follows:
        // Either "$root"
        //  - OR -
        // Start with either a letter or digit:                       ^[a-z0-9]
        // Followed by: 2-62 characters that comply to:               (...){2,62}$
        //   These inner characters can be either:
        //     A letter or digit:                                     ([a-z0-9]|
        //     Or a dash surround by a letter or digit on both sides: (?<=[a-z0-9])-(?=[a-z0-9])
        //http://msdn.microsoft.com/en-us/library/windowsazure/dd135715.aspx
        public static bool IsValidContainerName(string s)
        {
            Regex regex = new Regex(@"^\$root$|^[a-z0-9]([a-z0-9]|(?<=[a-z0-9])-(?=[a-z0-9])){2,62}$");
            return regex.IsMatch(s);
        }

        public static bool IsValidContainerPrefix(string s)
        {
            if (s.Length < 3)
            {
                s = s + "abc";
            };
            return IsValidContainerName(s);
        }

        //A blob name can contain any combination of characters,
        //but reserved URL characters must be properly escaped.
        //A blob name must be at least one character long and cannot be more than 1,024 characters long.
        //http://msdn.microsoft.com/en-us/library/windowsazure/dd135715.aspx
        public static bool IsValidBlobName(string s)
        {
            Regex regex = new Regex(@"^[\s\S]{1,1024}$");
            return regex.IsMatch(s);
        }

        //http://msdn.microsoft.com/en-us/library/windowsazure/dd179338.aspx
        public static bool IsValidTableName(string s)
        {
            Regex regex = new Regex(@"^[A-Za-z][A-Za-z0-9]{2,62}$");
            return regex.IsMatch(s);
        }

        public static bool IsValidTablePrefix(string s)
        {
            if (s.Length < 3)
            {
                s = s + "abc";
            };
            return IsValidTableName(s);
        }

        //http://msdn.microsoft.com/en-us/library/windowsazure/dd179349.aspx
        public static bool IsValidQueueName(string s)
        {
            Regex regex = new Regex(@"^[0-9a-z][a-z0-9-]{1,61}[0-9a-z]$");
            return regex.IsMatch(s);
        }

        public static bool IsValidQueuePrefix(string s)
        {
            if (s.Length < 3)
            {
                s = s + "abc";
            };
            return IsValidQueueName(s);
        }

        public static bool IsValidFileName(string s)
        {
            return !string.IsNullOrEmpty(s) && s.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1;
        }
    }
}
