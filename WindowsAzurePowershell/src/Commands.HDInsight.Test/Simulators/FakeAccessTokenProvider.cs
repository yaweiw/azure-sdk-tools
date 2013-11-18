namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Tests.Simulators
{
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
    using Microsoft.WindowsAzure.Management.HDInsight.Test.Simulators;

    internal class FakeAccessTokenProvider : ITokenProvider
    {
        private IAccessToken accessToken;

        internal FakeAccessTokenProvider(string token)
        {
            this.accessToken = new FakeAccessToken()
                {
                    AccessToken = token
                };
        }

        public IAccessToken GetCachedToken(WindowsAzureSubscription subscription, string userId)
        {
            return this.accessToken;
        }

        public IAccessToken GetNewToken(WindowsAzureEnvironment environment)
        {
            return this.accessToken;
        }

        public IAccessToken GetNewToken(WindowsAzureSubscription subscription, string userId)
        {
            return this.accessToken;
        }
    }
}