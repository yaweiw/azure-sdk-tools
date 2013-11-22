using System;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.HDInsight.Simulators
{
    internal class FakeAccessToken : IAccessToken
    {
        public void AuthorizeRequest(Action<string, string> authTokenSetter)
        {
        }

        public string AccessToken { get; internal set; }
        public string UserId { get; internal set; }
        public LoginType LoginType { get; internal set; }
    }
}
