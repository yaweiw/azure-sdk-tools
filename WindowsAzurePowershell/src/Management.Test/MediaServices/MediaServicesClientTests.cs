using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.Utilities.Common;
using Microsoft.WindowsAzure.Management.Utilities.MediaService;
using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services.MediaServicesEntities;
using Microsoft.WindowsAzure.ServiceManagement;

namespace Microsoft.WindowsAzure.Management.Test.MediaServices
{
    [TestClass]
    public class MediaServicesClientTests
    {
        private string _accountName = "testacc";
        private string _subscriptionId = "foo";

        [TestMethod]
        public void TestDeleteAzureMediaServiceAccountAsync()
        {
            HttpClient fakeHttpClient = new FakeHttpMessageHandler().CreateIMediaServicesHttpClient();

            var target = new MediaServicesClient(new SubscriptionData
            {
                SubscriptionId = _subscriptionId
            },
                null,
                fakeHttpClient,
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

            HttpClient fakeHttpClient = fakeHttpHandler.CreateIMediaServicesHttpClient();

            var target = new MediaServicesClient(new SubscriptionData
            {
                SubscriptionId = _subscriptionId
            },
                null,
                fakeHttpClient,
                fakeHttpClient);

            try
            {
                bool result = target.DeleteAzureMediaServiceAccountAsync(_accountName).Result;
            }
            catch (AggregateException ax)
            {
                var x = (ServiceManagementClientException) ax.InnerExceptions.Single();
                Assert.AreEqual("NotFound", x.ErrorDetails.Code);
                return;
            }

            Assert.Fail("ServiceManagementClientException expected");
        }

        [TestMethod]
        public void TestRegenerateMediaServicesAccountAsync()
        {
            HttpClient fakeHttpClient = new FakeHttpMessageHandler().CreateIMediaServicesHttpClient();

            var target = new MediaServicesClient(new SubscriptionData
            {
                SubscriptionId = _subscriptionId
            },
                null,
                fakeHttpClient,
                fakeHttpClient);

            bool result = target.RegenerateMediaServicesAccountAsync(_accountName, "Primary").Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestGetMediaServiceAsync()
        {
            var fakeHttpHandler = new FakeHttpMessageHandler();

            string responseText = "{\"AccountKey\":\"primarykey\",\"AccountKeys\":{\"Primary\":\"primarykey\",\"Secondary\":\"secondarykey\"},\"AccountName\":\"testps\",\"AccountRegion\":\"West US\",\"StorageAccountName\":\"nimbusorigintrial\"}";


            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new FakeHttpContent(responseText)
            };

            fakeHttpHandler.Send = request => response;

            HttpClient fakeHttpClient = fakeHttpHandler.CreateIMediaServicesHttpClient();

            var target = new MediaServicesClient(new SubscriptionData
            {
                SubscriptionId = _subscriptionId
            },
                null,
                fakeHttpClient,
                fakeHttpClient);

            MediaServiceAccountDetails result = target.GetMediaServiceAsync(_accountName).Result;
            Assert.AreEqual("primarykey", result.MediaServicesPrimaryAccountKey);
            Assert.AreEqual("secondarykey", result.MediaServicesSecondaryAccountKey);
            Assert.AreEqual("testps", result.Name);
        }

        [TestMethod]
        public void TestGetMediaServiceAccountsAsync()
        {
            var fakeHttpHandler = new FakeHttpMessageHandler();

            string responseText =
                "[{\"Name\":\"testps\",\"Type\":\"MediaService\",\"State\":\"Active\",\"AccountId\":\"E0658294-5C96-4B0F-AD55-F7446CE4F788\"},{\"Name\":\"test2\",\"Type\":\"MediaService\",\"State\":\"Active\",\"AccountId\":\"C92B17C8-5422-4CD1-8D3C-61E576E861DD\"}]";


            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new FakeHttpContent(responseText)
            };

            fakeHttpHandler.Send = request => response;

            HttpClient fakeHttpClient = fakeHttpHandler.CreateIMediaServicesHttpClient();

            var target = new MediaServicesClient(new SubscriptionData
            {
                SubscriptionId = _subscriptionId
            },
                null,
                fakeHttpClient,
                fakeHttpClient);

            MediaServiceAccount[] result = target.GetMediaServiceAccountsAsync().Result.ToArray();
            Assert.AreEqual(Guid.Parse("E0658294-5C96-4B0F-AD55-F7446CE4F788"), result[0].AccountId);
            Assert.AreEqual(Guid.Parse("C92B17C8-5422-4CD1-8D3C-61E576E861DD"), result[1].AccountId);
        }
    }
}