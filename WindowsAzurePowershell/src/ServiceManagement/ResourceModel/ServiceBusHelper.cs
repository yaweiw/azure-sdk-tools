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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel
{
    class ServiceBusBodyWriter : BodyWriter
    {
        string body;

        public ServiceBusBodyWriter(string body)
            : base(true)
        {
            this.body = body;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            XmlReader r = XmlReader.Create(new StringReader(body));
            writer.WriteNode(r, false);
        }
    }

    public class GetNamespaceFormatter : IClientMessageFormatter
    {
        private IClientMessageFormatter originalFormatter;

        public GetNamespaceFormatter(IClientMessageFormatter originalFormatter)
        {
            this.originalFormatter = originalFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            Namespace serviceBusNamespace = new Namespace();
            string XNamespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect";

            XDocument response = XDocument.Parse(message.ToString());
            XElement element = response.Descendants(XName.Get("NamespaceDescription", XNamespace)).First<XElement>();
            serviceBusNamespace.Name = element.Element(XName.Get("Name", XNamespace)).Value;
            serviceBusNamespace.Region = element.Element(XName.Get("Region", XNamespace)).Value;
            serviceBusNamespace.DefaultKey = element.Element(XName.Get("DefaultKey", XNamespace)).Value;
            serviceBusNamespace.Status = element.Element(XName.Get("Status", XNamespace)).Value;
            serviceBusNamespace.CreatedAt = element.Element(XName.Get("CreatedAt", XNamespace)).Value;
            serviceBusNamespace.AcsManagementEndpoint = new Uri(element.Element(XName.Get("AcsManagementEndpoint", XNamespace)).Value);
            serviceBusNamespace.ServiceBusEndpoint = new Uri(element.Element(XName.Get("ServiceBusEndpoint", XNamespace)).Value);
            serviceBusNamespace.ConnectionString = element.Element(XName.Get("ConnectionString", XNamespace)).Value;
            
            return serviceBusNamespace;
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return originalFormatter.SerializeRequest(messageVersion, parameters);
            //string body = "<foo>bar</foo>";
            //message msg1 = originalformatter.serializerequest(messageversion, parameters);
            //message msg = message.createmessage(messageversion, null, new ServiceBusBodyWriter(body));
            //msg.headers.copyheadersfrom(msg1);
            //msg.properties.copyproperties(msg1.properties);
        }
    }

    public class GetNamespaceBehaviorAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            // Do nothing.
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            clientOperation.Formatter = new GetNamespaceFormatter(clientOperation.Formatter);
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
