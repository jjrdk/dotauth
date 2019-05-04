namespace SimpleAuth
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Primitives;

    internal static class HttpRequestsExtensions
    {
        public static string GetAbsoluteUriWithVirtualPath(this HttpRequest requestMessage)
        {
            var host = requestMessage.Host.Host;

            var uri = new UriBuilder(requestMessage.Scheme, host);
            if (requestMessage.Host.Port.HasValue)
            {
                uri.Port = requestMessage.Host.Port.Value;
            }

            if (requestMessage.PathBase.HasValue)
            {
                uri.Path = requestMessage.PathBase.Value;
            }

            return uri.Uri.AbsoluteUri.TrimEnd('/');
        }

        public static async Task<string> ReadAsStringAsync(this HttpRequest request)
        {
            request.Body.Position = 0;
            using (var reader = new StreamReader(request.Body))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        public static X509Certificate2 GetCertificate(this HttpRequest request)
        {
            const string headerName = "X-ARR-ClientCert";
            var header = request.Headers.FirstOrDefault(h => h.Key == headerName);
            if (header.Equals(default(KeyValuePair<string, StringValues>)))
            {
                return null;
            }

            try
            {
                var encoded = Convert.FromBase64String(header.Value);
                return new X509Certificate2(encoded);
            }
            catch
            {
                return null;
            }
        }
    }
}