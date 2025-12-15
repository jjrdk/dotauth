namespace DotAuth.Extensions;

using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

internal static class HttpRequestsExtensions
{
    extension(HttpContext httpContext)
    {
        public ActionContext GetActionContext()
        {
            var endpoint = httpContext.GetEndpoint();
            var actionDescriptor =
                endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>() ??
                new ActionDescriptor();

            return new ActionContext(
                httpContext,
                httpContext.GetRouteData(),
                actionDescriptor
            );
        }
    }

    extension(HttpRequest requestMessage)
    {
        public Uri GetAbsoluteUri()
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

        public string GetAbsoluteUriWithVirtualPath()
        {
            return requestMessage.GetAbsoluteUri().AbsoluteUri.TrimEnd('/');
        }

        public X509Certificate2? GetCertificate()
        {
            const string headerName = "X-ARR-ClientCert";
            if (!requestMessage.Headers.TryGetValue(headerName, out var header))
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
}
