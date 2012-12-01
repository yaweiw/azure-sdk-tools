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

    public class GetServiceBusNamespaceFormatter : IClientMessageFormatter
    {
        private IClientMessageFormatter originalFormatter;

        public GetServiceBusNamespaceFormatter(IClientMessageFormatter originalFormatter)
        {
            this.originalFormatter = originalFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            XDocument response = XDocument.Parse(message.ToString());
            string serviceBusXNamespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect";
            XElement namespaceDescription = response.Descendants(XName.Get("NamespaceDescription", serviceBusXNamespace)).First<XElement>();

            return ServiceBusNamespace.Create(namespaceDescription);
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return originalFormatter.SerializeRequest(messageVersion, parameters);
        }
    }

    public class GetServiceBusNamespaceBehaviorAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            // Do nothing.
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            clientOperation.Formatter = new GetServiceBusNamespaceFormatter(clientOperation.Formatter);
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            throw new NotImplementedException();
        }

        public void Validate(OperationDescription operationDescription)
        {

        }
    }

    public class ListServiceBusNamespacesFormatter : IClientMessageFormatter
    {
        private IClientMessageFormatter originalFormatter;

        public ListServiceBusNamespacesFormatter(IClientMessageFormatter originalFormatter)
        {
            this.originalFormatter = originalFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            XDocument response = XDocument.Parse(message.ToString());
            string serviceBusXNamespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect";
            ServiceBusNamespaceList namespaces = new ServiceBusNamespaceList();
            IEnumerable<XElement> subscriptionNamespaces = response.Descendants(XName.Get("NamespaceDescription", serviceBusXNamespace));

            foreach (XElement namespaceDescription in subscriptionNamespaces)
            {
                namespaces.Add(ServiceBusNamespace.Create(namespaceDescription));
            }

            return namespaces;
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return originalFormatter.SerializeRequest(messageVersion, parameters);
        }
    }

    public class ListServiceBusNamespacesBehaviorAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            // Do nothing.
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            clientOperation.Formatter = new ListServiceBusNamespacesFormatter(clientOperation.Formatter);
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
