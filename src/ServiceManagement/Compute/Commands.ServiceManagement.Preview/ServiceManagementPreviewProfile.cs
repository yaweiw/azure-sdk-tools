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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Preview
{
    using AutoMapper;
    using Management.Network.Models;
    using Model;
    using System;

    public class ServiceManagementPreviewProfile : Profile
    {
        private static readonly Lazy<bool> initialize;

        static ServiceManagementPreviewProfile()
        {
            initialize = new Lazy<bool>(() =>
            {
                Mapper.AddProfile<ServiceManagementPreviewProfile>();
                return true;
            });
        }

        public static bool Initialize()
        {
            return ServiceManagementProfile.Initialize() && initialize.Value;
        }

        public override string ProfileName
        {
            get { return "ServiceManagementPreviewProfile"; }
        }

        protected override void Configure()
        {
            // Reserved IP
            Mapper.CreateMap<OperationStatusResponse, ReservedIPContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()))
                  .ForMember(c => c.Id, o => o.Ignore());

            Mapper.CreateMap<NetworkReservedIPGetResponse, ReservedIPContext>()
                  .ForMember(c => c.ReservedIPName, o => o.MapFrom(r => r.Name));

            Mapper.CreateMap<NetworkReservedIPListResponse.ReservedIP, ReservedIPContext>()
                  .ForMember(c => c.ReservedIPName, o => o.MapFrom(r => r.Name));
        }
    }
}