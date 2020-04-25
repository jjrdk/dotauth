namespace SimpleAuth.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    
    /// <summary>
    /// Defines the base client for interacting with an authorization server.
    /// </summary>
    public abstract class ClientBase
    {
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientBase"/> class.
        /// </summary>
        /// <param name="client"></param>
        protected ClientBase(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Gets the result of the <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of data downloaded.</typeparam>
        /// <param name="request">The download request.</param>
        /// <param name="token">The authorization token for the request.</param>
        /// <param name="certificate">The <see cref="X509Certificate2"/> to include in request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        protected async Task<GenericResponse<T>> GetResult<T>(
            HttpRequestMessage request,
            string token,
            CancellationToken cancellationToken = default,
            X509Certificate2 certificate = null)
        {
            request = PrepareRequest(request, token);
            var result = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (result.IsSuccessStatusCode)
            {
                return new GenericResponse<T>
                {
                    StatusCode = result.StatusCode,
                    Content = Serializer.Default.Deserialize<T>(content)
                };
            }

            var genericResult = new GenericResponse<T>
            {
                Error = string.IsNullOrWhiteSpace(content)
                    ? new ErrorDetails { Status = result.StatusCode }
                    : Serializer.Default.Deserialize<ErrorDetails>(content),
                StatusCode = result.StatusCode
            };

            return genericResult;
        }

        private static HttpRequestMessage PrepareRequest(
            HttpRequestMessage request,
            string authorizationHeaderValue,
            X509Certificate2 certificate = null)
        {
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    JwtBearerConstants.BearerScheme,
                    authorizationHeaderValue);
            }

            if (certificate != null)
            {
                var base64Encoded = Convert.ToBase64String(certificate.RawData);
                request.Headers.Add("X-ARR-ClientCert", base64Encoded);
            }

            return request;
        }
    }
}