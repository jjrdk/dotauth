namespace SimpleAuth.Manager.Client.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Results;
    using Shared.Responses;

    internal sealed class GetConfigurationOperation : IGetConfigurationOperation
    {
        private readonly Dictionary<string, DiscoveryInformation> _cache = new Dictionary<string, DiscoveryInformation>();
        private readonly HttpClient _httpClientFactory;

        public GetConfigurationOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetConfigurationResult> ExecuteAsync(Uri wellKnownConfigurationUri)
        {
            if (wellKnownConfigurationUri == null)
            {
                throw new ArgumentNullException(nameof(wellKnownConfigurationUri));
            }

            if (_cache.TryGetValue(wellKnownConfigurationUri.ToString(), out var doc))
            {
                return new GetConfigurationResult
                {
                    Content = doc
                };
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = wellKnownConfigurationUri
            };
            var httpResult = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            //httpResult.EnsureSuccessStatusCode();
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                httpResult.EnsureSuccessStatusCode();

                return new GetConfigurationResult
                {
                    Content = JsonConvert.DeserializeObject<DiscoveryInformation>(content)
                };
            }
            catch (Exception)
            {
                return new GetConfigurationResult
                {
                    ContainsError = true,
                    HttpStatus = httpResult.StatusCode
                };
            }
        }
    }
}
