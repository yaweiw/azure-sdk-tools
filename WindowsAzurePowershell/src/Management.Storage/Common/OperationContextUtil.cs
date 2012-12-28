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
// ---------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// operation context utility
    /// </summary>
    public static class OperationContextUtil
    {
        /// <summary>
        /// init storage client operation context
        /// </summary>
        public static void Init(this OperationContext operationContext)
        {
            operationContext.StartTime = DateTime.Now;
            operationContext.ClientRequestID = operationContext.GetClientRequestID();
        }

        /// <summary>
        /// get an unique client request id
        /// </summary>
        /// <returns></returns>
        public static string GetClientRequestID(this OperationContext operationContext)
        {
            string uniqueId = System.Guid.NewGuid().ToString();
            return string.Format(Resources.ClientRequestIdFormat, uniqueId);
        }

        /// <summary>
        /// get the running ms from when operationcontext started
        /// </summary>
        /// <returns></returns>
        public static double GetRunningMilliseconds(this OperationContext operationContext)
        {
            TimeSpan span = DateTime.Now - operationContext.StartTime;
            return span.TotalMilliseconds;
        }
    }
}
