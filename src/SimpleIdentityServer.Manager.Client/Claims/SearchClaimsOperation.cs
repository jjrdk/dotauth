namespace SimpleAuth.Manager.Client.Claims
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Results;
    using Shared.Requests;
    using Shared.Responses;

    internal sealed class SearchClaimsOperation : ISearchClaimsOperation
    {
        private readonly HttpClient _httpClientFactory;

        public SearchClaimsOperation(HttpClient httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PagedResult<ClaimResponse>> ExecuteAsync(Uri claimsUri, SearchClaimsRequest parameter, string authorizationHeaderValue = null)
        {
            if (claimsUri == null)
            {
                throw new ArgumentNullException(nameof(claimsUri));
            }

            var serializedPostPermission = JsonConvert.SerializeObject(parameter);
            var body = new StringContent(serializedPostPermission, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = claimsUri,
                Content = body
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
                return new PagedResult<ClaimResponse>
                {
                    ContainsError = true,
                    HttpStatus = httpResult.StatusCode,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content)
                };
            }

            return new PagedResult<ClaimResponse>
            {
                Content = JsonConvert.DeserializeObject<PagedResponse<ClaimResponse>>(content)
            };
        }
    }
}
