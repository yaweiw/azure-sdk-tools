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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.PlatformImageRepository
{
    using System;
    using AutoMapper;
    using ExtensionPublishing;
    using Management.Compute.Models;

    public class ServiceManagementPlatformImageRepositoryProfile : Profile
    {
        private static readonly Lazy<bool> initialize;

        static ServiceManagementPlatformImageRepositoryProfile()
        {
            initialize = new Lazy<bool>(() =>
            {
                Mapper.AddProfile<ServiceManagementPlatformImageRepositoryProfile>();
                return true;
            });
        }

        public static bool Initialize()
        {
            return ServiceManagementProfile.Initialize() && initialize.Value;
        }

        public override string ProfileName
        {
            get { return "ServiceManagementPlatformImageRepositoryProfile"; }
        }

        protected override void Configure()
        {
            Mapper.CreateMap<PublishAzurePlatformExtensionCommand, ExtensionImageRegisterParameters>()
                  .ForMember(c => c.IsJsonExtension, o => o.MapFrom(r => !r.XmlExtension.IsPresent))
                  .ForMember(c => c.Type, o => o.MapFrom(r => r.ExtensionName))
                  .ForMember(c => c.ProviderNameSpace, o => o.MapFrom(r => r.Publisher))
                  .ForMember(c => c.IsInternalExtension, o => o.MapFrom(r => true))
                  .ForMember(c => c.BlockRoleUponFailure, o => o.MapFrom(r => r.BlockRoleUponFailure.IsPresent))
                  .ForMember(c => c.DisallowMajorVersionUpgrade, o => o.MapFrom(r => r.DisallowMajorVersionUpgrade.IsPresent));

            Mapper.CreateMap<UpdateAzurePlatformExtensionCommand, ExtensionImageUpdateParameters>()
                  .ForMember(c => c.Type, o => o.MapFrom(r => r.ExtensionName))
                  .ForMember(c => c.ProviderNameSpace, o => o.MapFrom(r => r.Publisher));
        }
    }
}