namespace SimpleIdentityServer.Core
{
    using System.Net.Http.Headers;
    using Common.DTOs.Responses;
    using Common.Models;
    using Parameters;
    using Results;

    public interface IPayloadSerializer
    {
        string GetPayload(AuthorizationParameter parameter);
        string GetPayload(IntrospectionParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(IntrospectionResult parameter);
        string GetPayload(RegistrationParameter parameter);
        string GetPayload(ClientRegistrationResponse parameter);
        string GetPayload(string accessToken);
        string GetPayload(UserInfoResult parameter);
        string GetPayload(AuthorizationCodeGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(ClientCredentialsGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(RefreshTokenGrantTypeParameter parameter);
        string GetPayload(ResourceOwnerGrantTypeParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(RevokeTokenParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
        string GetPayload(GrantedToken parameter);
        string GetPayload(Results.ActionResult parameter);
    }
}