namespace SimpleAuth
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.IO;
    using System.Threading.Tasks;

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
    }
}