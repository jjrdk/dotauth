namespace SimpleAuth
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Security.Cryptography.X509Certificates;

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

        public static X509Certificate2? GetCertificate(this HttpRequest request)
        {
            const string headerName = "X-ARR-ClientCert";
            if (!request.Headers.TryGetValue(headerName, out var header))
            {
                return null;
            }

            try
            {
                var encoded = Convert.FromBase64String(header);
                return new X509Certificate2(encoded);
            }
            catch
            {
                return null;
            }
        }
    }
}