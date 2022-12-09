namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class SmsLoginFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public SmsLoginFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario(DisplayName = "Successfully receive token using sms login")]
    public void SuccessfulSmsAuthentication()
    {
        TokenClient client = null!;
        GrantedTokenResponse result = null!;

        "and a properly configured token client".x(
            () => client = new TokenClient(
                TokenCredentials.FromBasicAuthentication("client", "client"),
                Fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting an sms".x(
            async () =>
            {
                var response = await client.RequestSms(new ConfirmationCodeRequest {PhoneNumber = "phone"})
                    .ConfigureAwait(false);

                Assert.IsType<Option.Success>(response);
            });

        "and then requesting token".x(
            async () =>
            {
                var response = await client
                    .GetToken(TokenRequest.FromPassword("phone", "123", new[] {"openid"}, "sms"))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                result = response.Item;
            });

        "then has valid access token".x(
            () =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    IssuerSigningKeys = ServerKeyset.GetSigningKeys(),
                    ValidAudience = "client",
                    ValidIssuer = "https://localhost"
                };
                tokenHandler.ValidateToken(result.AccessToken, validationParameters, out var token);

                Assert.NotEmpty(((JwtSecurityToken) token).Claims);
            });

        "and has valid id token".x(
            () =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = TestKeys.SecretKey.CreateJwk(
                        JsonWebKeyUseNames.Sig,
                        KeyOperations.Sign,
                        KeyOperations.Verify),
                    ValidAudience = "client",
                    ValidIssuer = "https://localhost"
                };
                tokenHandler.ValidateToken(result.IdToken, validationParameters, out _);
            });
    }

}