namespace SimpleIdentityServer.Manager.Client.Claims
{
    using Newtonsoft.Json;
    using Results;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Responses;

    internal sealed class GetAllClaimsOperation : IGetAllClaimsOperation
    {
        private readonly HttpClient _httpClientFactory;

        public GetAllClaimsOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GetAllClaimsResult> ExecuteAsync(Uri claimsUri, string authorizationHeaderValue = null)
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
            try
            {
                httpResult.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetAllClaimsResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = httpResult.StatusCode
                };
            }

            return new GetAllClaimsResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<ClaimResponse>>(content)
            };
        }
    }
}
