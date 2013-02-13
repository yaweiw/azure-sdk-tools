/**
* Copyright Microsoft Corporation  2012
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

namespace SMClientTest
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.Text;
    using Microsoft.WindowsAzure.ServiceManagement;

    class Program
    {
        static void Main(string[] args)
        {
            // Create the binding
            Console.WriteLine("Creating binding.");
            WebHttpBinding binding = null;
            binding = new WebHttpBinding(WebHttpSecurityMode.Transport);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

            binding.ReceiveTimeout = TimeSpan.FromHours(1);
            binding.SendTimeout = TimeSpan.FromMinutes(1);
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);

            // TODO: Copy and paste the subscription ID as shown in the Portal
            string subscriptionId = "<SUBSCRIPTION ID>";
            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Using Subscription ID: {0}.", subscriptionId));

            // Set the URI of the endpoint to call Service Management API at
            Uri smEndpointUri = ServiceManagementClient.ServiceManagementUri; 
            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Using Service Management endpoint: {0}.", smEndpointUri));

            // Load the client certificate
            X509Certificate2 clientCert = Program.GetCertificate();
            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Using client certificate: {0}.", clientCert.Thumbprint));

            // Configure a TraceListener from configuration
            TraceSource tracer = new TraceSource("SMTestApp", SourceLevels.Verbose);

            // Set the client options - This is where you can hook up a custom UserAgent or x-ms-client-id header as well as configure logging
            ServiceManagementClientOptions clientOptions = new ServiceManagementClientOptions("SMTestClient", null, tracer, 0);

            // Make sure to wrap ServiceManagementClient in a using statement since it is IDisposable
            using (ServiceManagementClient smClient = new ServiceManagementClient(binding, smEndpointUri, clientCert, clientOptions))
            {
                // Initialize the client
                Console.WriteLine("Initializing the client.");

                // Make a call to get the subscription
                Console.WriteLine("Calling GetSubscription.");
                Subscription mySub = smClient.Service.GetSubscription(subscriptionId);
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Get subscription returned subscription with name: {0}.", mySub.SubscriptionName));

                // Try to create a Hosted Service without all the fields populated and expect a failure
                try
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Attempting to create an invalid Hosted Service.");
                    smClient.Service.CreateHostedService(subscriptionId, new CreateHostedServiceInput()
                    {
                        ServiceName = "MyTestService",
                    });
                }
                catch (ServiceManagementClientException ex)
                {
                    string smErrorCode = ((ex.ErrorDetails != null) && (!string.IsNullOrEmpty(ex.ErrorDetails.Code))) ? ex.ErrorDetails.Code : "<NONE>";
                    string smErrorMessage = ((ex.ErrorDetails != null) && (!string.IsNullOrEmpty(ex.ErrorDetails.Message))) ? ex.ErrorDetails.Message : "<NONE>";
                    StringBuilder headers = new StringBuilder();
                    if (ex.ResponseHeaders != null)
                    {
                        foreach (string header in ex.ResponseHeaders.AllKeys)
                        {
                            headers.AppendFormat("{0}:{1},", header, ex.ResponseHeaders[header]);
                        }
                    }

                    Console.WriteLine(string.Format(CultureInfo.CurrentCulture, 
                        "Error creating hosted service. HTTP Status: {0}. SM Error Code: {1}. Message: {2}. Headers: {3}.", (int)ex.HttpStatus, smErrorCode, smErrorMessage, 
                        headers.ToString()));
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                // Now try to run an asyc operation that we need to wait for like create storage account. 
                // This call will poll and wait for the operation to complete.
                // A failure on the service side will result in the same ServiceManagementClientException convention as if calling a synchronous operation.
                string storageService1Name = "testss" + Guid.NewGuid().ToString("N").ToLowerInvariant().Substring(0, 15); // Create a random DNS name that meets storage service requirements
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Attempting to create a storage service with name {0}.", storageService1Name));
                smClient.RunAsyncRequestAndWaitForCompletion(subscriptionId, TimeSpan.FromMinutes(10), () =>
                    {
                        smClient.Service.CreateStorageService(subscriptionId, new CreateStorageServiceInput()
                        {
                            ServiceName = storageService1Name,
                            Label = ServiceManagementClient.EncodeToBase64String("MyStorage1"),
                            Location = "uswest",
                        });                       
                    });
                Console.WriteLine("Storage service created.");

                // Cleanup the newly created storage service
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Deleting storage service with name {0}.", storageService1Name));
                smClient.Service.DeleteStorageService(subscriptionId, storageService1Name);

                // There are two ways to handle async calls. The first one (demonstrated above) retrieves the x-ms-request-id and does the waiting all in one step.
                // If you'd like more control or you only need to fetch the ID but not wait or you have an ID already and would like to wait, you can do so as follows.
                // First call the Service Management API and get the tracking ID
                string storageService2Name = "testss" + Guid.NewGuid().ToString("N").ToLowerInvariant().Substring(0, 15); // Create a random DNS name that meets storage service requirements
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Attempting to create a storage service with name {0}.", storageService2Name));
                string trackingId = smClient.RunRequestAndGetTrackingId(() =>
                {
                    smClient.Service.CreateStorageService(subscriptionId, new CreateStorageServiceInput()
                    {
                        ServiceName = storageService2Name,
                        Label = ServiceManagementClient.EncodeToBase64String("MyStorage2"),
                        Location = "uswest",
                    });
                });

                // Now wait for the operation to complete
                smClient.WaitForOperationToComplete(subscriptionId, trackingId, TimeSpan.FromMinutes(10));
                Console.WriteLine("Storage service created.");
                
                // Again, cleanup the storage service                
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Deleting storage service with name {0}.", storageService2Name));
                smClient.Service.DeleteStorageService(subscriptionId, storageService2Name);
            }

            Console.WriteLine("Finished with Service Management client sample. Cleaning up.");
        }

        private static X509Certificate2 GetCertificate()
        {
            // TODO: Replace this method with code to load your management certificate that you uploaded to the portal
            
            X509Certificate2 clientCert = null;

            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            try
            {
                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection matchingCerts = certStore.Certificates.Find(X509FindType.FindByThumbprint, "a9be06e8881339d7b69f8421f51304431bed0ee7", true);
                if ((matchingCerts != null) && (matchingCerts.Count > 0))
                {
                    clientCert = matchingCerts[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not load certificate: {0}.", ex));
            }
            finally
            {
                if (certStore != null)
                {
                    certStore.Close();
                }
            }
            return clientCert;
        }
    }
}
