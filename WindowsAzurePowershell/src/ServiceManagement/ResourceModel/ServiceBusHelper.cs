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
    public class ServiceBusConstants
    {
        public const string ServiceBusXNamespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect";
    }

    public class ServiceBusBodyWriter : BodyWriter
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
            XElement namespaceDescription = response.Descendants(XName.Get("NamespaceDescription", ServiceBusConstants.ServiceBusXNamespace)).First<XElement>();

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
            ServiceBusNamespaceList namespaces = new ServiceBusNamespaceList();
            IEnumerable<XElement> subscriptionNamespaces = response.Descendants(XName.Get("NamespaceDescription", ServiceBusConstants.ServiceBusXNamespace));

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

    public class ListServiceBusRegionsFormatter : IClientMessageFormatter
    {
        private IClientMessageFormatter originalFormatter;

        public ListServiceBusRegionsFormatter(IClientMessageFormatter originalFormatter)
        {
            this.originalFormatter = originalFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            XDocument response = XDocument.Parse(message.ToString());
            ServiceBusRegionList regions = new ServiceBusRegionList();
            IEnumerable<XElement> subscriptionRegions = response.Descendants(XName.Get("RegionCodeDescription", ServiceBusConstants.ServiceBusXNamespace));

            foreach (XElement regionDescription in subscriptionRegions)
            {
                regions.Add(ServiceBusRegion.Create(regionDescription));
            }

            return regions;
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return originalFormatter.SerializeRequest(messageVersion, parameters);
        }
    }

    public class ListServiceBusRegionsBehaviorAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            // Do nothing.
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            clientOperation.Formatter = new ListServiceBusRegionsFormatter(clientOperation.Formatter);
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            throw new NotImplementedException();
        }

        public void Validate(OperationDescription operationDescription)
        {

        }
    }

    public class CreateServiceBusNamespaceFormatter : IClientMessageFormatter
    {
        private IClientMessageFormatter originalFormatter;

        public CreateServiceBusNamespaceFormatter(IClientMessageFormatter originalFormatter)
        {
            this.originalFormatter = originalFormatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            XDocument response = XDocument.Parse(message.ToString());
            XElement namespaceDescription = response.Descendants(XName.Get("NamespaceDescription", ServiceBusConstants.ServiceBusXNamespace)).First<XElement>();

            return ServiceBusNamespace.Create(namespaceDescription);
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            ServiceBusNamespace namespaceDescription = parameters[1] as ServiceBusNamespace;
            string body = CreateRequestBody(namespaceDescription);
            Message originalMessage = originalFormatter.SerializeRequest(messageVersion, parameters);
            Message updatedMessage = Message.CreateMessage(messageVersion, null, new ServiceBusBodyWriter(body));
            updatedMessage.Headers.CopyHeadersFrom(originalMessage);
            updatedMessage.Properties.CopyProperties(originalMessage.Properties);
            HttpRequestMessageProperty property = updatedMessage.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            property.Headers.Add("Content-Type", "application/xml");
            property.Headers.Add("type", "entry");
            property.Headers.Add("charset", "utf-8");

            return updatedMessage;
        }

        private string CreateRequestBody(ServiceBusNamespace namespaceDescription)
        {
            return new XDocument(
                new XElement(XName.Get("NamespaceDescription", ServiceBusConstants.ServiceBusXNamespace),
                    new XElement(XName.Get("Name", ServiceBusConstants.ServiceBusXNamespace), namespaceDescription.Name),
                    new XElement(XName.Get("Region", ServiceBusConstants.ServiceBusXNamespace), namespaceDescription.Region)
                    )).ToString();
        }
    }

    public class CreateServiceBusNamespaceBehaviorAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            // Do nothing.
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            clientOperation.Formatter = new CreateServiceBusNamespaceFormatter(clientOperation.Formatter);
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
