// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Helpers
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Converter;
    using Errors;
    using Exceptions;
    using Shared;
    using Shared.Requests;

    public class JsonWebKeyHelper : IJsonWebKeyHelper
    {
        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;
        private readonly HttpClient _httpClientFactory;

        public JsonWebKeyHelper(
            IJsonWebKeyConverter jsonWebKeyConverter,
            HttpClient httpClientFactory)
        {
            _jsonWebKeyConverter = jsonWebKeyConverter;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JsonWebKey> GetJsonWebKey(string kid, Uri uri)
        {
            if (string.IsNullOrWhiteSpace(kid))
            {
                throw new ArgumentNullException(nameof(kid));
            }

            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            try
            {
                var request = await _httpClientFactory.GetAsync(uri.AbsoluteUri).ConfigureAwait(false);
                request.EnsureSuccessStatusCode();
                var json = request.Content.ReadAsStringAsync().Result;
                var jsonWebKeySet = json.DeserializeWithJavascript<JsonWebKeySet>();
                var jsonWebKeys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                return jsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            }
            catch (Exception)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, uri.AbsoluteUri));
            }
        }
    }
}
