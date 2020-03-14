namespace SimpleAuth.ResourceServer
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    internal static class TicketResponseExtensions
    {
        public static void ConfigureResponse(
            this HttpResponse response,
            GenericResponse<PermissionResponse> permissionResponse,
            Uri umaAuthority,
            string realm)
        {
            if (permissionResponse.ContainsError)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Headers[HeaderNames.Warning] = "199 - \"UMA Authorization Server Unreachable\"";
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                var s = string.IsNullOrWhiteSpace(realm) ? string.Empty : "realm=\"{Options.Realm}\", ";
                response.Headers[HeaderNames.WWWAuthenticate] =
                    $"UMA {s}as_uri=\"{umaAuthority}\", ticket=\"{permissionResponse.Content.TicketId}\"";
            }
        }
    }
}