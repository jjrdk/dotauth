namespace SimpleIdentityServer.Manager.Client.Claims
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Results;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Responses;

    internal sealed class GetClaimOperation : IGetClaimOperation
    {
        private readonly HttpClient _httpClientFactory;

        public GetClaimOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetClaimResult> ExecuteAsync(Uri claimsUri, string authorizationHeaderValue = null)
        {
            if (claimsUri == null)
            {
                throw new ArgumentNullException(nameof(claimsUri));
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = claimsUri
            };
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var httpResult = await _httpClientFactory.SendAsync(request).ConfigureAwait(false);
            var content = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);
            var rec = JObject.Parse(content);
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetClaimResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }
            
            return new GetClaimResult
            {
                Content = JsonConvert.DeserializeObject<ClaimResponse>(content)
            };
        }
    }
}
