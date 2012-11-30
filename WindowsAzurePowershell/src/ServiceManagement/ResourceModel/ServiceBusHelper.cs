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
            XDocument response = XDocument.Parse(message.ToString());
            string serviceBusXNamespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect";
            XElement namespaceDescription = response.Descendants(XName.Get("NamespaceDescription", serviceBusXNamespace)).First<XElement>();

            return Namespace.Create(namespaceDescription);
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return originalFormatter.SerializeRequest(messageVersion, parameters);
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

    public class ListNamespacesFormatter : IClientMessageFormatter
    {
        private IClientMessageFormatter originalFormatter;

        public ListNamespacesFormatter(IClientMessageFormatter originalFormatter)
        {
            this.originalFormatter = originalFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            XDocument response = XDocument.Parse(message.ToString());
            string serviceBusXNamespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect";
            NamespaceList namespaces = new NamespaceList();
            IEnumerable<XElement> subscriptionNamespaces = response.Descendants(XName.Get("NamespaceDescription", serviceBusXNamespace));

            foreach (XElement namespaceDescription in subscriptionNamespaces)
            {
                namespaces.Add(Namespace.Create(namespaceDescription));
            }

            return namespaces;
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return originalFormatter.SerializeRequest(messageVersion, parameters);
        }
    }

    public class ListNamespacesBehaviorAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            // Do nothing.
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            clientOperation.Formatter = new ListNamespacesFormatter(clientOperation.Formatter);
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
