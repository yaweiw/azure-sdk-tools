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

namespace Microsoft.WindowsAzure.Management.Websites.Services.DeploymentEntities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Utilities;

    /// <summary>
    /// Log.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Log
    {
    }

    /// <summary>
    /// Collection of logs.
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Logs : List<Log>
    {

        /// <summary>
        /// Empty collection.
        /// </summary>
        public Logs() { }

        /// <summary>
        /// Initialize collection.
        /// </summary>
        /// <param name="logs"></param>
        public Logs(List<Log> logs) : base(logs) { }
    }
}
