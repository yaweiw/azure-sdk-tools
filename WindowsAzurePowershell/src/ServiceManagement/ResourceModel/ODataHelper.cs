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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.IO;
    using System.ServiceModel.Description;
    using System.Xml.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using Microsoft.Data.OData;
    using System.Net;

    public class ODataFormatter<T> : IClientMessageFormatter where T : class, IODataResolvable, new()
    {
        private IClientMessageFormatter originalFormatter;

        public ODataFormatter(IClientMessageFormatter originalFormatter)
        {
            this.originalFormatter = originalFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            List<T> resultList = null;
            T resultEntry = null;
            ODataMessageReaderSettings readerSettings = new ODataMessageReaderSettings();
            readerSettings.MessageQuotas = new ODataMessageQuotas();
            MemoryStream memoryStream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(memoryStream);
            message.WriteMessage(writer);
            writer.Flush();
            memoryStream.Position = 0;
            Stream responseStream = memoryStream;
            var sr = new StreamReader(memoryStream);

            using (ODataMessageReader responseReader = new ODataMessageReader(new HttpResponseAdapterMessage(message, responseStream), readerSettings))
            {
                ODataReader reader;

                List<ODataPayloadKindDetectionResult> payloadKind = new List<ODataPayloadKindDetectionResult>(responseReader.DetectPayloadKind());

                if (payloadKind.Any<ODataPayloadKindDetectionResult>(r => r.PayloadKind == ODataPayloadKind.Entry))
                {
                    reader = responseReader.CreateODataEntryReader();
                    ReadResult(reader, out resultEntry);
                }
                else if (payloadKind.Any<ODataPayloadKindDetectionResult>(r => r.PayloadKind == ODataPayloadKind.Feed))
                {
                    reader = responseReader.CreateODataFeedReader();
                    ReadResult(reader, out resultList);
                }
            }

            return (resultEntry != null) ? resultEntry : (object)resultList;
        }

        private void ReadResult(ODataReader reader, out List<T> resultList)
        {
            resultList = new List<T>();

            // Start => FeedStart
            if (reader.State == ODataReaderState.Start)
            {
                ODataFeed feed = (ODataFeed)reader.Item;
                reader.Read();
            }

            // Feedstart 
            if (reader.State == ODataReaderState.FeedStart)
            {
                ODataFeed feed = (ODataFeed)reader.Item;
                reader.Read();
            }

            while (reader.State == ODataReaderState.EntryStart)
            {
                do
                {
                    reader.Read();
                } while (!(reader.Item is ODataEntry));

                ODataEntry entry = (ODataEntry)reader.Item;
                T item = new T();
                item.Resolve(entry);
                resultList.Add(item);

                reader.Read();
            }
        }

        private void ReadResult(ODataReader reader, out T item)
        {
            item = new T();

            while (reader.Read())
            {
                if (reader.State == ODataReaderState.EntryEnd)
                {
                    ODataEntry entry = (ODataEntry)reader.Item;
                    item.Resolve(entry);
                }
            }
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return originalFormatter.SerializeRequest(messageVersion, parameters);
        }
    }

    public class ODataBehaviorAttribute : Attribute, IOperationBehavior
    {
        private Type dataContractType;

        public ODataBehaviorAttribute(Type formatterType)
        {
            this.dataContractType = formatterType;
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            // Do nothing.
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            Type genericFormatterType = typeof(ODataFormatter<>);
            Type formatterType = genericFormatterType.MakeGenericType(new Type[] { dataContractType });
            ConstructorInfo ctor = formatterType.GetConstructor(new Type[] { typeof(IClientMessageFormatter) });
            IClientMessageFormatter newFormatter = ctor.Invoke(new object[] { clientOperation.Formatter }) as IClientMessageFormatter;
            clientOperation.Formatter = newFormatter;
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            throw new NotImplementedException();
        }

        public void Validate(OperationDescription operationDescription)
        {

        }
    }
}
