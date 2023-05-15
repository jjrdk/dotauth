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

namespace DotAuth.Shared;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Responses;

internal sealed class GetDiscoveryOperation
{
    private readonly SemaphoreSlim _semaphore = new(1);

    private readonly Dictionary<string, DiscoveryInformation> _cache = new();

    private readonly Uri _discoveryDocumentationUri;
    private readonly Func<HttpClient> _httpClient;

    public GetDiscoveryOperation(Uri authority, Func<HttpClient> httpClient)
    {
        const string wellKnownOpenidConfiguration = "/.well-known/openid-configuration";
        var path = authority.PathAndQuery;
        if (!string.IsNullOrWhiteSpace(path))
        {
            var hasWellKnownPart = path.IndexOf("/.well-known", StringComparison.OrdinalIgnoreCase);
            if (hasWellKnownPart > -1)
            {
                path = path[..hasWellKnownPart];
            }

            var queryStart = path.IndexOf('?');
            if (queryStart > 0)
            {
                path = path[..queryStart].TrimEnd('/') + wellKnownOpenidConfiguration;
            }
            else
            {
                path = path.TrimEnd('/') + wellKnownOpenidConfiguration;
            }
        }
        else
        {
            path = wellKnownOpenidConfiguration;
        }

        var uri = new UriBuilder(
            authority.Scheme,
            authority.Host,
            authority.Port,
            path);
        _discoveryDocumentationUri = uri.Uri;
        _httpClient = httpClient;
    }

    public async Task<DiscoveryInformation> Execute(CancellationToken cancellationToken = default)
    {
        var key = _discoveryDocumentationUri.ToString();
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_cache.TryGetValue(key, out var doc))
            {
                return doc;
            }

            var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = _discoveryDocumentationUri };
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response =
                await _httpClient().SendAsync(request, cancellationToken).ConfigureAwait(false);
            var serializedContent = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            doc = await JsonSerializer.DeserializeAsync<DiscoveryInformation>(serializedContent,
                DefaultJsonSerializerOptions.Instance, cancellationToken: cancellationToken);
            _cache.Add(key, doc!);
            return doc!;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
