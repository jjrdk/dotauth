namespace DotAuth.Extensions;

using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;

internal static class HttpRequestsExtensions
{
    public static Uri GetAbsoluteUri(this HttpRequest requestMessage)
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

        return uri.Uri;
    }

    public static string GetAbsoluteUriWithVirtualPath(this HttpRequest requestMessage)
    {
        return requestMessage.GetAbsoluteUri().AbsoluteUri.TrimEnd('/');
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
            var encoded = Convert.FromBase64String(header!);
            return X509CertificateLoader.LoadCertificate(encoded);
        }
        catch
        {
            return null;
        }
    }
}
