﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Newtonsoft.Json;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Operations
{
    using System.Net.Http;

    public interface IGetDiscoveryOperation
    {
        Task<DiscoveryInformation> ExecuteAsync(Uri discoveryDocumentationUri);
    }

    internal class GetDiscoveryOperation : IGetDiscoveryOperation
    {
        private readonly HttpClient _httpClient;

        #region Constructor

        public GetDiscoveryOperation(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #endregion

        #region Public methods

        public async Task<DiscoveryInformation> ExecuteAsync(Uri discoveryDocumentationUri)
        {
            if (discoveryDocumentationUri == null)
            {
                throw new ArgumentNullException(nameof(discoveryDocumentationUri));
            }

            var serializedContent = await _httpClient.GetStringAsync(discoveryDocumentationUri).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<DiscoveryInformation>(serializedContent);
        }

        #endregion
    }
}
