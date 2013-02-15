/**
* Copyright Microsoft Corporation 2012
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Microsoft.WindowsAzure.ServiceManagement
{
    using System;
    using System.Globalization;

    internal static class ArgumentValidator
    {
        /// <summary>
        /// Throws an exception if the argument is null.
        /// </summary>
        /// <param name="argumentName">The name of the argument being supplied.</param>
        /// <param name="argumentValue">The value of the argument being supplied.</param>
        public static void CheckIfNull(string argumentName, object argumentValue)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName, string.Format(CultureInfo.InvariantCulture, Resources.ArgumentNullError, argumentName));
            }
        }

        /// <summary>
        /// Throws an exception if the string argument is empty.
        /// </summary>
        /// <param name="argumentName">The name of the string argument being supplied.</param>
        /// <param name="argumentValue">The value of the string argument being supplied.</param>
        public static void CheckIfEmptyString(string argumentName, string argumentValue)
        {
            if (argumentValue.Length <= 0)
            {
                throw new ArgumentException(argumentName, string.Format(CultureInfo.InvariantCulture, Resources.StringArgumentEmptyError, argumentName));
            }
        }
    }
}
