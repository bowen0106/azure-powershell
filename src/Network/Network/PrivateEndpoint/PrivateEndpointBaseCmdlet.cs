﻿// ----------------------------------------------------------------------------------
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

using Microsoft.Azure.Commands.Network.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Management.Network.Models;
using Microsoft.Rest.Azure;
using System.Net;

namespace Microsoft.Azure.Commands.Network
{
    public abstract class PrivateEndpointBaseCmdlet : NetworkBaseCmdlet
    {
        public IPrivateEndpointsOperations PrivateEndpointClient
        {
            get
            {
                return NetworkClient.NetworkManagementClient.PrivateEndpoints;
            }
        }

        public bool IsPrivateEndpointPresent(string resourceGroupName, string name)
        {
            try
            {
                GetPrivateEndpoint(resourceGroupName, name);
            }
            catch (ErrorException exception)
            {
                if (exception.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Resource is not present
                    return false;
                }

                throw;
            }

            return true;
        }

        public PSPrivateEndpoint GetPrivateEndpoint(string resourceGroupName, string name, string expandResource = null)
        {
            var privateEndpoint = this.PrivateEndpointClient.Get(resourceGroupName, name, expandResource);

            var psPrivateEndpoint = ToPsPrivateEndpoint(privateEndpoint);
            psPrivateEndpoint.ResourceGroupName = resourceGroupName;

            return psPrivateEndpoint;
        }

        public PSPrivateEndpoint ToPsPrivateEndpoint(PrivateEndpoint privateEndpoint)
        {
            var psPrivateEndpoint = NetworkResourceManagerProfile.Mapper.Map<PSPrivateEndpoint>(privateEndpoint);
            psPrivateEndpoint.Tag = TagsConversionHelper.CreateTagHashtable(privateEndpoint.Tags);
            return psPrivateEndpoint;
        }

    }
}
