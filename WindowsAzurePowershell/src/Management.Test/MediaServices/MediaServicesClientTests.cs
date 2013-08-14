using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.Utilities.Common;
using Microsoft.WindowsAzure.Management.Utilities.MediaService;
using Microsoft.WindowsAzure.ServiceManagement;

namespace Microsoft.WindowsAzure.Management.Test.MediaServices
{
    [TestClass]
    public class MediaServicesClientTests
    {
        private string _subscriptionId = "foo";
        private string _accountName = "testacc";

        [TestMethod]
        public void TestDeleteAzureMediaServiceAccountAsync()
        {
            var fakeHttpClient = new FakeHttpMessageHandler().CreateIMediaServicesHttpClient();

            var target = new MediaServicesClient(
                new SubscriptionData { SubscriptionId = _subscriptionId }, 
                null,
                fakeHttpClient);

            bool result = target.DeleteAzureMediaServiceAccountAsync(_accountName).Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestDeleteAzureMediaServiceAccountAsync404()
        {
            var fakeHttpHandler = new FakeHttpMessageHandler();

            string responseText = "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">{\"Code\":\"NotFound\",\"Message\":\"The specified account was not found.\"}</string>";

            var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new FakeHttpContent(responseText)
            };

            fakeHttpHandler.Send = request => response;

            var fakeHttpClient = fakeHttpHandler.CreateIMediaServicesHttpClient();

            var target = new MediaServicesClient(
                new SubscriptionData { SubscriptionId = _subscriptionId },
                null,
                fakeHttpClient);

            try
            {
                bool result = target.DeleteAzureMediaServiceAccountAsync(_accountName).Result;
            }
            catch (AggregateException ax)
            {
                ServiceManagementClientException x = (ServiceManagementClientException)ax.InnerExceptions.Single();
                Assert.AreEqual("NotFound", x.ErrorDetails.Code);
            }
        }

        [TestMethod]
        public void TestRegenerateMediaServicesAccountAsync()
        {
            var fakeHttpClient = new FakeHttpMessageHandler().CreateIMediaServicesHttpClient();

            var target = new MediaServicesClient(
                new SubscriptionData { SubscriptionId = _subscriptionId },
                null,
                fakeHttpClient);

            bool result = target.RegenerateMediaServicesAccountAsync(_accountName, "Primary").Result;

            Assert.IsTrue(result);
        }
    }
}
