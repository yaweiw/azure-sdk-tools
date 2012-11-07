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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
#if SERVER
    using Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement;
#endif

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class SubscriptionOperationCollection : IExtensibleDataObject
    {
        [DataMember(Order = 0)]
        public SubscriptionOperationList SubscriptionOperations { get; set; }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string ContinuationToken { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Namespace = Constants.ServiceManagementNS, Name = "SubscriptionOperations", ItemName = "SubscriptionOperation")]
    public class SubscriptionOperationList : List<SubscriptionOperation>, IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }

        public SubscriptionOperationList()
        {
        }

        public SubscriptionOperationList(IEnumerable<SubscriptionOperation> subscriptions)
            : base(subscriptions)
        {
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class SubscriptionOperationCaller : IExtensibleDataObject
    {
        [DataMember(Order = 0)]
        public bool UsedServiceManagementApi { get; set; }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string UserEmailAddress { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string SubscriptionCertificateThumbprint { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string ClientIP { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Namespace = Constants.ServiceManagementNS, Name = "OperationParameters", ItemName = "OperationParameter")]
    public class OperationParameterList : List<OperationParameter>, IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }

        public OperationParameterList()
        {
        }

        public OperationParameterList(IEnumerable<OperationParameter> operations)
            : base(operations)
        {
        }
    }

    /// <summary>
    /// Represents a parameter for operation.
    /// </summary>
    [DataContract]
    public class OperationParameter : IExtensibleDataObject
    {
        /// <summary>
        /// Name of the parameter, return value for the operation will have name
        /// as @return.
        /// </summary>
        [DataMember(Order = 0)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        private string Value { get; set; }

        private static Type[] KnownTypes = 
        {
            typeof(CreateAffinityGroupInput),
            typeof(UpdateAffinityGroupInput),
            typeof(CertificateFile),
            typeof(ChangeConfigurationInput),
            typeof(CreateDeploymentInput),
            typeof(CreateHostedServiceInput),
            typeof(CreateStorageServiceInput),
            typeof(PrepareImageUploadInput),
            typeof(RegenerateKeys),
            typeof(SetMachineImagePropertiesInput),
            typeof(SetParentImageInput),
            typeof(StorageDomain),
            typeof(SubscriptionCertificate),
            typeof(SwapDeploymentInput),
            typeof(UpdateDeploymentStatusInput),
            typeof(UpdateHostedServiceInput),
            typeof(UpdateStorageServiceInput),
            typeof(UpgradeDeploymentInput),
            typeof(WalkUpgradeDomainInput),
            typeof(CaptureRoleOperation),
            typeof(ShutdownRoleOperation),
            typeof(StartRoleOperation),
            typeof(RestartRoleOperation),
            typeof(OSImage),
            typeof(PersistentVMRole),
            typeof(Deployment),
            typeof(DataVirtualHardDisk),
            typeof(OSImage),
            typeof(Disk),
            typeof(ExtendedProperty)
        };

        /// <summary>
        /// Helper to retrieve the serialized value of the parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string GetSerializedValue()
        {
            return this.Value;
        }

        /// <summary>
        /// Helper to retrieve the value with it's type being discoverable
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            DataContractSerializer serializer;

            if (string.IsNullOrEmpty(this.Value))
            {
                return null;
            }

            serializer = new DataContractSerializer(typeof(object), OperationParameter.KnownTypes);

            try
            {
                return serializer.ReadObject(XmlReader.Create(new StringReader(this.Value)));
            }
            catch
            {
                // If we can't deserialize for some reason, we give back the raw XML
                return this.Value;
            }
        }

        public void SetValue(object value)
        {
            if (value != null)
            {
                Type valueType = value.GetType();

                ////Avoid serialization for string datatype.
                if (valueType.Equals(typeof(string)))
                {
                    this.Value = (string)value;
                    return;
                }

                DataContractSerializer serializer = new DataContractSerializer(typeof(object), OperationParameter.KnownTypes);
                StringBuilder target = new StringBuilder();
                using (XmlWriter writer = XmlWriter.Create(target))
                {
                    serializer.WriteObject(writer, value);
                    writer.Flush();
                    this.Value = target.ToString();
                }
            }
        }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class SubscriptionOperation : IExtensibleDataObject
    {
        [DataMember(Order = 0)]
        public string OperationId { get; set; }

        [DataMember(Order = 1)]
        public string OperationObjectId { get; set; }

        [DataMember(Order = 2)]
        public string OperationName { get; set; }

        [DataMember(Order = 3)]
        public OperationParameterList OperationParameters { get; set; }

        [DataMember(Order = 4)]
        public SubscriptionOperationCaller OperationCaller { get; set; }

        [DataMember(Order = 5)]
        public Operation OperationStatus { get; set; }

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public string OperationStartedTime { get; set; }

        [DataMember(Order = 8, EmitDefaultValue = false)]
        public string OperationCompletedTime { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Subscription : IExtensibleDataObject
    {
        [DataMember(Order = 0)]
        public string SubscriptionID { get; set; }

        [DataMember(Order = 1)]
        public string SubscriptionName { get; set; }

        [DataMember(Order = 2)]
        public string SubscriptionStatus { get; set; }

        [DataMember(Order = 3)]
        public string AccountAdminLiveEmailId { get; set; }

        [DataMember(Order = 4)]
        public string ServiceAdminLiveEmailId { get; set; }

        [DataMember(Order = 5)]
        public int MaxCoreCount { get; set; }

        [DataMember(Order = 6)]
        public int MaxStorageAccounts { get; set; }

        [DataMember(Order = 7)]
        public int MaxHostedServices { get; set; }

        [DataMember(Order = 8)]
        public int CurrentCoreCount { get; set; }

        [DataMember(Order = 9)]
        public int CurrentHostedServices { get; set; }

        [DataMember(Order = 10)]
        public int CurrentStorageAccounts { get; set; }

        [DataMember(Order = 11, EmitDefaultValue = false)]
        public int MaxVirtualNetworkSites { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        public int CurrentVirtualNetworkSites { get; set; }

        [DataMember(Order = 13, EmitDefaultValue = false)]
        public int MaxLocalNetworkSites { get; set; }

        [DataMember(Order = 14, EmitDefaultValue = false)]
        public int CurrentLocalNetworkSites { get; set; }

        [DataMember(Order = 15, EmitDefaultValue = false)]
        public int MaxDnsServers { get; set; }

        [DataMember(Order = 16, EmitDefaultValue = false)]
        public int CurrentDnsServers { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
