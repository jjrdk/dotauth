namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    internal sealed class GetConfigurationOperation
    {
        private readonly Dictionary<string, DiscoveryInformation> _cache =
            new Dictionary<string, DiscoveryInformation>();

        private readonly HttpClient _httpClient;

        public GetConfigurationOperation(HttpClient httpClientFactory)
        {
            _httpClient = httpClientFactory;
        }

        public async Task<GenericResponse<DiscoveryInformation>> Execute(Uri wellKnownConfigurationUri)
        {
            if (wellKnownConfigurationUri == null)
            {
                throw new ArgumentNullException(nameof(wellKnownConfigurationUri));
            }

            if (_cache.TryGetValue(wellKnownConfigurationUri.ToString(), out var doc))
            {
                return new GenericResponse<DiscoveryInformation> {Content = doc};
            }

            var request = new HttpRequestMessage {Method = HttpMethod.Get, RequestUri = wellKnownConfigurationUri};
            var httpResult = await _httpClient.SendAsync(request).ConfigureAwait(false);
            httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();

                var discoveryInformation = JsonConvert.DeserializeObject<DiscoveryInformation>(content);
                _cache.Add(wellKnownConfigurationUri.ToString(), discoveryInformation);
                return new GenericResponse<DiscoveryInformation> {Content = discoveryInformation};
            }
            catch (Exception)
            {
                return new GenericResponse<DiscoveryInformation>
                {
                    ContainsError = true, HttpStatus = httpResult.StatusCode
                };
            }
        }
    }
}
