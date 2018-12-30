namespace SimpleAuth.Manager.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Results;
    using Shared;
    using Shared.Requests;
    using Shared.Responses;

    public sealed class ProfileClient : IProfileClient
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public ProfileClient(HttpClient client)
        {
            _client = client;
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public Task<BaseResponse> LinkMyProfile(string requestUrl, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException(nameof(requestUrl));
            }

            if (linkProfileRequest == null)
            {
                throw new ArgumentNullException(nameof(linkProfileRequest));
            }

            var url = requestUrl + "/.me";
            return Link(url, linkProfileRequest, authorizationHeaderValue);
        }

        public Task<BaseResponse> LinkProfile(string requestUrl, string currentSubject, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            if (linkProfileRequest == null)
            {
                throw new ArgumentNullException(nameof(linkProfileRequest));
            }


            var url = requestUrl + $"/{currentSubject}";
            return Link(url, linkProfileRequest, authorizationHeaderValue);
        }

        private async Task<BaseResponse> Link(string requestUrl, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null)
        {
            var json = JsonConvert.SerializeObject(
                linkProfileRequest,
                _jsonSerializerSettings);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json),
                RequestUri = new Uri(requestUrl)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationHeaderValue);
            }

            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new BaseResponse();
        }

        public Task<BaseResponse> UnlinkProfile(string requestUrl, string externalSubject, string currentSubject, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }

            if (string.IsNullOrWhiteSpace(currentSubject))
            {
                throw new ArgumentNullException(nameof(currentSubject));
            }

            var url = requestUrl + $"/{currentSubject}/{externalSubject}";
            return Delete(url, authorizationHeaderValue);
        }

        public Task<BaseResponse> UnlinkMyProfile(string requestUrl, string externalSubject, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new ArgumentNullException(nameof(externalSubject));
            }

            var url = requestUrl + $"/.me/{externalSubject}";
            return Delete(url, authorizationHeaderValue);
        }

        private async Task<BaseResponse> Delete(string url, string authorizationValue = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationValue);
            }

            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new BaseResponse
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new BaseResponse();
        }

        public Task<GetProfilesResult> GetProfiles(string requestUrl, string currentSubject, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            if (string.IsNullOrWhiteSpace(currentSubject))
            {
                throw new ArgumentNullException(nameof(currentSubject));
            }

            requestUrl += $"/{currentSubject}";
            return GetAll(requestUrl, authorizationHeaderValue);
        }

        public Task<GetProfilesResult> GetMyProfiles(string requestUrl, string authorizationHeaderValue = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentNullException(nameof(requestUrl));
            }

            requestUrl += "/.me";
            return GetAll(requestUrl, authorizationHeaderValue);
        }

        private async Task<GetProfilesResult> GetAll(string url, string authorizationValue = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };
            if (!string.IsNullOrWhiteSpace(authorizationValue))
            {
                request.Headers.Add("Authorization", "Bearer " + authorizationValue);
            }

            var result = await _client.SendAsync(request).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return new GetProfilesResult
                {
                    ContainsError = true,
                    Error = JsonConvert.DeserializeObject<ErrorResponse>(content),
                    HttpStatus = result.StatusCode
                };
            }

            return new GetProfilesResult
            {
                Content = JsonConvert.DeserializeObject<IEnumerable<ProfileResponse>>(content)
            };
        }

    }
}
