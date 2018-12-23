namespace SimpleIdentityServer.Manager.Client.Configuration
{
    using Newtonsoft.Json;
    using Results;
    using Shared.Responses;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal sealed class GetConfigurationOperation : IGetConfigurationOperation
    {
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
                    Content = JsonConvert.DeserializeObject<ConfigurationResponse>(content)
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
