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
using System.Security;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

namespace Microsoft.WindowsAzure.Commands.Common
{
    public interface IAuthenticationFactory
    {
        IAccessToken Authenticate(AzureEnvironment environment, ref UserCredentials credentials);
        IAccessToken Authenticate(AzureEnvironment environment, string tenant, ref UserCredentials credentials);
        SubscriptionCloudCredentials GetSubscriptionCloudCredentials(Guid subscriptionId);
    }
}
