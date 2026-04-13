namespace DotAuth.Endpoints;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using DotAuth.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal static class DeviceUiEndpointHandlers
{
    internal static IResult Get(string code)
    {
        return Results.Json(new DeviceAuthorizationViewModel { Code = code });
    }

    internal static async Task<IResult> Approve(
        HttpContext httpContext,
        IDeviceAuthorizationStore deviceAuthorizationStore,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var code = await TryGetCodeAsync(httpContext.Request, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(code))
        {
            return Results.BadRequest();
        }

        var authorization = await deviceAuthorizationStore.Approve(code, cancellationToken).ConfigureAwait(false);
        if (authorization is Option.Error e)
        {
            loggerFactory.CreateLogger("DotAuth.Controllers.DeviceController")
                .LogError("User code: {Code} not found", code);
            return Results.Json(
                new ErrorViewModel
                {
                    Code = (int)HttpStatusCode.BadRequest,
                    Title = e.Details.Title,
                    Message = e.Details.Detail
                },
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Json(new object());
    }

    private static async Task<string?> TryGetCodeAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (request.Query.TryGetValue("code", out var queryCode))
        {
            return queryCode.ToString();
        }

        if (!request.HasFormContentType)
        {
            return null;
        }

        var form = await request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
        return form.TryGetValue("code", out var formCode) ? formCode.ToString() : null;
    }
}


