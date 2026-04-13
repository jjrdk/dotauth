namespace DotAuth.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

internal sealed class SmsUiEndpointRegistration : IDotAuthUiEndpointRegistration
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sms/authenticate", SmsUiEndpointHandlers.GetAuthenticateIndex).WithOrder(-1);
        endpoints.MapPost("/sms/authenticate/locallogin", SmsUiEndpointHandlers.PostLocalLogin).WithOrder(-1);
        endpoints.MapGet("/sms/authenticate/confirmcode", SmsUiEndpointHandlers.GetConfirmCode).WithOrder(-1);
        endpoints.MapPost("/sms/authenticate/confirmcode", SmsUiEndpointHandlers.PostConfirmCode).WithOrder(-1);
        endpoints.MapPost("/sms/authenticate/localloginopenid", SmsUiEndpointHandlers.PostLocalLoginOpenId).WithOrder(-1);
        endpoints.MapPost("/code", SmsUiEndpointHandlers.SendConfirmationCode).WithOrder(-1);
    }
}

