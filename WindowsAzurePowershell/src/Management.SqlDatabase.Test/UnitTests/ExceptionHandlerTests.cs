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

using System;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.CloudService.Test;
using Microsoft.WindowsAzure.Management.SqlDatabase.Services;

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.Server.Cmdlet
{
    [TestClass]
    public class ExceptionHandlerTests : TestBase
    {
        public static readonly string MockServerPrefix = "http://localhost:12345/MockTestServer/";

        [TestMethod]
        public void ServiceResourceErrorTest()
        {
            string serverRequestId = Guid.NewGuid().ToString();

            string errorMessage = @"<Error xmlns=""Microsoft.SqlServer.Management.Framework.Web.Services"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Message>Resource with the name 'FirewallRule1' does not exist. To continue, specify a valid resource name.</Message><InnerError i:nil=""true""/></Error>";
            WebException exception = CreateWebException(HttpStatusCode.NotFound, errorMessage, (context) =>
            {
                context.Response.Headers.Add(Constants.RequestIdHeaderName, serverRequestId);
            });

            ErrorRecord errorRecord;
            string requestId;
            SqlDatabaseManagementHelper.RetrieveExceptionDetails(exception, out errorRecord, out requestId);

            Assert.AreEqual(serverRequestId, requestId);
            Assert.AreEqual("Resource with the name 'FirewallRule1' does not exist. To continue, specify a valid resource name.", errorRecord.Exception.Message);
        }

        [TestMethod]
        public void SqlDatabaseManagementErrorTest()
        {
            string serverRequestId = Guid.NewGuid().ToString();

            string errorMessage = 
@"<Error xmlns=""http://schemas.microsoft.com/sqlazure/2010/12/"">
  <Code>40647</Code>
  <Message>Subscription '00000000-1111-2222-3333-444444444444' does not have the server 'server0001'.</Message>
  <Severity>16</Severity>
  <State>1</State>
</Error>";
            WebException exception = CreateWebException(HttpStatusCode.BadRequest, errorMessage, (context) =>
            {
                context.Response.Headers.Add(Constants.RequestIdHeaderName, serverRequestId);
            });

            ErrorRecord errorRecord;
            string requestId;
            SqlDatabaseManagementHelper.RetrieveExceptionDetails(exception, out errorRecord, out requestId);

            Assert.AreEqual(serverRequestId, requestId);
            Assert.AreEqual(
@"Subscription '00000000-1111-2222-3333-444444444444' does not have the server 'server0001'.
Error Code: 40647", 
                  errorRecord.Exception.Message);
        }

        private static WebException CreateWebException(HttpStatusCode status, string content, Action<HttpListenerContext> contextHandler)
        {
            return CreateWebException(status, new MemoryStream(Encoding.UTF8.GetBytes(content)), contextHandler);
        }

        private static WebException CreateWebException(HttpStatusCode status, MemoryStream content, Action<HttpListenerContext> contextHandler)
        {
            HttpListener server = null;
            try
            {
                // Create a mock server that always returns the response code and exception stream specified in the parameter.
                server = CreateMockServer((context) =>
                {
                    contextHandler(context);
                    context.Response.StatusCode = (int)status;
                    content.Position = 0;
                    content.CopyTo(context.Response.OutputStream);
                    context.Response.Close();
                }, 1);

                WebClient client = new WebClient();
                try
                {
                    client.OpenRead(ExceptionHandlerTests.MockServerPrefix + "exception.htm");
                }
                catch (WebException ex)
                {
                    return ex;
                }
            }
            finally
            {
                server.Stop();
            }

            return null;
        }

        private static HttpListener CreateMockServer(Action<HttpListenerContext> contextHandler, int requestsToHandle)
        {
            // Create a mock http listener.
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(MockServerPrefix);
            listener.Start();
            listener.BeginGetContext((ar) => HandleRequest(ar, contextHandler, requestsToHandle - 1), listener);
            return listener;
        }

        private static void HandleRequest(IAsyncResult ar, Action<HttpListenerContext> contextHandler, int requestsToHandle)
        {
            HttpListener listener = (HttpListener)ar.AsyncState;

            // Get the current context, request and response object and construct the response.
            HttpListenerContext context = listener.EndGetContext(ar);
            contextHandler(context);

            if (requestsToHandle > 0)
            {
                try
                {
                    // Setup the next context
                    listener.BeginGetContext((arNext) => HandleRequest(arNext, contextHandler, requestsToHandle - 1), listener);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
