namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;

    public class UmaTicketLoginFlowFeature : AuthFlowFeature
    {
        [Scenario(DisplayName = "Successful authorization")]
        public void SuccessfulTicketAuthentication()
        {
            AddResourceSetResponse resourceSetResponse = null;
            UmaClient umaClient = null;
            TokenClient client = null;
            GrantedTokenResponse result = null;
            string ticketId = null;

            "and a properly configured token client".x(
                async () => client = await TokenClient.Create(
                        TokenCredentials.FromClientCredentials("post_client", "post_client"),
                        _fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting token".x(
                async () =>
                {
                    var response = await client
                        .GetToken(TokenRequest.FromPassword("user", "password", new[] { "uma_protection" }))
                        .ConfigureAwait(false);
                    result = response.Content;
                });

            "then has valid access token".x(
                () =>
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = _jwks.GetSigningKeys(),
                        ValidAudience = "post_client",
                        ValidIssuer = "https://localhost"
                    };
                    tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                    Assert.NotEmpty(((JwtSecurityToken)token).Claims);
                });

            "given a uma client".x(
                async () =>
                {
                    umaClient = await UmaClient.Create(
                        _fixture.Client,
                        new Uri("https://localhost/.well-known/uma2-configuration"));
                });

            "when creating resource set".x(
                async () =>
                {
                    var resourceSet = new ResourceSet
                    {
                        Uri = "http://localhost",
                        Name = "Local",
                        Scopes = new[] { "api1" },
                        Type = "url"
                    };

                    var resourceResponse = await umaClient.AddResource(resourceSet, result.AccessToken);
                    resourceSetResponse = resourceResponse.Content;

                    Assert.False(resourceResponse.ContainsError);
                });

            "and setting permission".x(
                async () =>
                {
                    var request = new PostPermission { ResourceSetId = resourceSetResponse.Id, Scopes = new[] { "api1" } };
                    var permissionResponse = await umaClient.AddPermission(request, result.AccessToken);
                    ticketId = permissionResponse.Content.TicketId;

                    Assert.Null(permissionResponse.Error);
                });

            "when requesting ticket".x(
                async () =>
                {
                    var response = await client.GetToken(TokenRequest.FromTicketId(ticketId, result.IdToken))
                        .ConfigureAwait(false);
                    var umaToken = response.Content;

                    Assert.Null(response.Error);
                    Assert.NotNull(umaToken.AccessToken);
                });
        }
    }
}
