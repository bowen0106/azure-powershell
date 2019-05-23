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

using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using Microsoft.Azure.Commands.Network.Models;
using Microsoft.Azure.Management.Network;
using System.Linq;
using System.Management.Automation;
using MNM = Microsoft.Azure.Management.Network.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.New, ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "PrivateLinkService", SupportsShouldProcess = true), OutputType(typeof(PSPrivateLinkService))]
    public class NewAzurePrivateLinkService : PrivateLinkServiceBaseCmdlet
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The name of the service.",
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ServiceName { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "location.")]
        [LocationCompleter("Microsoft.Network/privateLinkServices")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(
           Mandatory = true,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "The front end ip configurations")]
        [ValidateNotNullOrEmpty]
        public PSFrontendIPConfiguration[] LoadBalancerFrontendIpConfigurations { get; set; }

        [Parameter(
           Mandatory = true,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "The ip configurations")]
        [ValidateNotNullOrEmpty]
        public PSPrivateLinkServiceIpConfiguration[] IpConfigurations { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not ask for confirmation if you want to overwrite a resource")]
        public SwitchParameter Force { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Run cmdlet in the background")]
        public SwitchParameter AsJob { get; set; }

        private PSPrivateLinkService CreatePSPrivateLinkService()
        {
            var psPrivateLinkService = new PSPrivateLinkService
            {
                Name = ServiceName,
                ResourceGroupName = ResourceGroupName,
                Location = Location
            };


            psPrivateLinkService.LoadBalancerFrontendIpConfigurations = LoadBalancerFrontendIpConfigurations?.ToList();
            psPrivateLinkService.IpConfigurations = IpConfigurations?.ToList();
            //psPrivateLinkService.ProvisioningState = "Succeeded";

            var plsModel = NetworkResourceManagerProfile.Mapper.Map<MNM.PrivateLinkService>(psPrivateLinkService);
            // plsModel.Tags = TagsConversionHelper.CreateTagDictionary(Tag, validate: true);

            this.PrivateLinkServiceClient.CreateOrUpdate(ResourceGroupName, ServiceName, plsModel);
            var getPrivateLinkService = GetPrivateLinkService(ResourceGroupName, ServiceName);

            return getPrivateLinkService;
        }

        public override void Execute()
        {
            base.Execute();
            var present = IsPrivateLinkServicePresent(ResourceGroupName, ServiceName);
            ConfirmAction(
                Force.IsPresent,
                string.Format(Properties.Resources.OverwritingResource, ServiceName),
                Properties.Resources.CreatingResourceMessage,
                ServiceName,
                () =>
                {
                    var privateLinkService = CreatePSPrivateLinkService();
                    WriteObject(privateLinkService);
                },
                () => present);
        }
    }
}