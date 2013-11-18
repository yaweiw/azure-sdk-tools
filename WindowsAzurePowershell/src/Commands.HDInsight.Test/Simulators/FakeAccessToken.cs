namespace Microsoft.WindowsAzure.Management.HDInsight.Test.Simulators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

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
