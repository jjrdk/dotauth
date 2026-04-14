namespace DotAuth.Server.Tests;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Requests;
using Xunit;

public sealed class ClientFixture : IDisposable
{
    private const string OpenIdManagerConfiguration = "http://localhost:5000/.well-known/openid-configuration";
    private readonly TestManagerServerFixture _server;
    private readonly ManagementClient _openidClients;

    public ClientFixture()
    {
        _server = new TestManagerServerFixture();
        _openidClients = ManagementClient.Create(_server.Client, new Uri(OpenIdManagerConfiguration)).GetAwaiter()
            .GetResult();
    }

    [Fact]
    public async Task When_Pass_No_Parameter_Then_Error_Is_Returned()
    {
        var result =
            Assert.IsType<Option<Client>.Error>(
                await _openidClients.AddClient(new Client(), "token", TestContext.Current.CancellationToken)
            );

        Assert.Equal(ErrorCodes.InvalidRedirectUri, result.Details.Title);
    }

    [Fact]
    public async Task When_Getting_Create_Client_Page_As_Html_Then_Razor_View_Is_Returned()
    {
        using var client = _server.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5000/{CoreConstants.EndPoints.Clients}/create");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "token");

        var response = await client.SendAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Create Client", content);
    }

    [Fact]
    public async Task When_Getting_Clients_As_Html_Then_Razor_View_Is_Returned()
    {
        using var client = _server.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5000/{CoreConstants.EndPoints.Clients}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "token");

        var response = await client.SendAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Registered Clients", content);
    }

    [Fact]
    public async Task When_Getting_Client_Details_As_Html_Then_Razor_View_Is_Returned()
    {
        var addedClient = Assert.IsType<Option<Client>.Result>(await _openidClients.AddClient(
            new Client
            {
                ClientId = Guid.NewGuid().ToString("N"),
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                AllowedScopes = ["openid"],
                ApplicationType = ApplicationTypes.Web,
                ClientName = "client_for_html_page",
                RedirectionUrls = [new Uri("http://localhost")]
            },
            "token",
            TestContext.Current.CancellationToken));

        using var client = _server.Server.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5000/{CoreConstants.EndPoints.Clients}/{addedClient.Item.ClientId}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerConstants.BearerScheme, "token");

        var response = await client.SendAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Client Info", content);
        Assert.Contains(addedClient.Item.ClientId, content);
    }

    [Fact]
    public async Task When_Add_User_And_Redirect_Uri_Contains_Fragment_Then_Error_Is_Returned()
    {
        var result = Assert.IsType<Option<Client>.Error>(await _openidClients.AddClient(
            new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                AllowedScopes = ["openid"],
                ClientId = "test",
                ClientName = "name",
                RedirectionUrls = [new Uri("http://localhost#fragment")]
            },
            "", TestContext.Current.CancellationToken)
        );

        Assert.Equal("invalid_redirect_uri", result.Details.Title);
        Assert.Equal("The redirect_uri http://localhost/#fragment cannot contain fragment", result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_And_Pass_No_Parameter_Then_Error_Is_Returned()
    {
        var result = Assert.IsType<Option<Client>.Error>(
            await _openidClients.UpdateClient(new Client(), "token", TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.InvalidRedirectUri, result.Details.Title);
        Assert.Equal(string.Format(Strings.MissingParameter, "redirect_uris"), result.Details.Detail);
    }

    [Fact]
    public async Task When_Update_Add_Pass_Invalid_Scopes_Then_Error_Is_Returned()
    {
        var client = new Client
        {
            ClientId = Guid.NewGuid().ToString("N"),
            JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
            AllowedScopes = ["openid"],
            ApplicationType = ApplicationTypes.Web,
            ClientName = "client_name",
            ClientUri = new Uri("http://clienturi.com"),
            Contacts = ["contact"],
            DefaultAcrValues = "sms",
            GrantTypes = [GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken],
            RedirectionUrls = [new Uri("http://localhost")],
            PostLogoutRedirectUris = [new Uri("http://localhost/callback")]
        };
        var addClientResult = Assert.IsType<Option<Client>.Result>(
            await _openidClients.AddClient(client, "token", TestContext.Current.CancellationToken));
        client = addClientResult.Item;
        client.AllowedScopes = ["not_valid"];
        var result =
            Assert.IsType<Option<Client>.Error>(
                await _openidClients.UpdateClient(client, "token", TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.InvalidScope, result.Details.Title);
        Assert.Equal("Unknown scopes: not_valid", result.Details.Detail);
    }

    [Fact]
    public async Task When_Get_Unknown_Client_Then_Error_Is_Returned()
    {
        var newClient = Assert.IsType<Option<Client>.Error>(
            await _openidClients.GetClient("unknown_client", "token", TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.InvalidRequest, newClient.Details.Title);
        Assert.Equal(SharedStrings.TheClientDoesntExist, newClient.Details.Detail);
    }

    [Fact]
    public async Task When_Delete_An_Unknown_Client_Then_Error_Is_Returned()
    {
        var newClient =
            await _openidClients.DeleteClient("unknown_client", "token", TestContext.Current.CancellationToken);

        Assert.IsType<Option<Client>.Error>(newClient);
    }

    [Fact]
    public async Task When_Add_Client_Then_Information_Is_Correct()
    {
        var client = new Client
        {
            ClientId = Guid.NewGuid().ToString("N"),
            JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
            AllowedScopes = ["openid"],
            ApplicationType = ApplicationTypes.Web,
            ClientName = "client_name",
            IdTokenSignedResponseAlg = "RS256",
            IdTokenEncryptedResponseAlg = "RSA1_5",
            IdTokenEncryptedResponseEnc = "A128CBC-HS256",
            UserInfoSignedResponseAlg = "RS256",
            UserInfoEncryptedResponseAlg = "RSA1_5",
            UserInfoEncryptedResponseEnc = "A128CBC-HS256",
            RequestObjectSigningAlg = "RS256",
            RequestObjectEncryptionAlg = "RSA1_5",
            RequestObjectEncryptionEnc = "A128CBC-HS256",
            TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
            InitiateLoginUri = new Uri("https://initloginuri"),
            ClientUri = new Uri("http://clienturi.com"),
            Contacts = ["contact"],
            DefaultAcrValues = "sms",
            GrantTypes = [GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken],
            ResponseTypes = [ResponseTypeNames.Code, ResponseTypeNames.IdToken, ResponseTypeNames.Token],
            RedirectionUrls = [new Uri("http://localhost")],
            PostLogoutRedirectUris = [new Uri("http://localhost/callback")],
            //LogoUri = new Uri("http://logouri.com")
        };
        var result =
            Assert.IsType<Option<Client>.Result>(await _openidClients.AddClient(client, "token",
                TestContext.Current.CancellationToken));

        var newClient = Assert.IsType<Option<Client>.Result>(
            await _openidClients.GetClient(result.Item.ClientId, "token", TestContext.Current.CancellationToken)
        );

        Assert.Equal(ApplicationTypes.Web, newClient.Item.ApplicationType);
        Assert.Equal("client_name", newClient.Item.ClientName);
        Assert.Equal(new Uri("http://clienturi.com"), newClient.Item.ClientUri);
        Assert.Equal("sms", newClient.Item.DefaultAcrValues);
        Assert.Single(newClient.Item.Contacts);
        Assert.Single(newClient.Item.RedirectionUrls);
        Assert.Single(newClient.Item.PostLogoutRedirectUris);
        Assert.Equal(3, newClient.Item.GrantTypes.Length);
        Assert.Equal(3, newClient.Item.ResponseTypes.Length);
    }

    [Fact]
    public async Task When_Update_Client_Then_Information_Are_Correct()
    {
        var client = new Client
        {
            ClientId = Guid.NewGuid().ToString("N"),
            JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
            AllowedScopes = ["openid"],
            ApplicationType = ApplicationTypes.Web,
            ClientName = "client_name",
            ClientUri = new Uri("http://clienturi.com"),
            Contacts = ["contact"],
            DefaultAcrValues = "sms",
            // DefaultMaxAge = 10,
            GrantTypes = [GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken],
            RedirectionUrls = [new Uri("http://localhost")],
            PostLogoutRedirectUris = [new Uri("http://localhost/callback")],
            //LogoUri = new Uri("http://logouri.com")
        };

        var addClientResult = Assert.IsType<Option<Client>.Result>(
            await _openidClients.AddClient(client, "token", TestContext.Current.CancellationToken));
        client = addClientResult.Item;
        client.PostLogoutRedirectUris =
        [
            new Uri("http://localhost/callback"), new Uri("http://localhost/callback2")
        ];
        client.GrantTypes = [GrantTypes.AuthorizationCode, GrantTypes.Implicit];
        var result =
            Assert.IsType<Option<Client>.Result>(
                await _openidClients.UpdateClient(client, "token", TestContext.Current.CancellationToken)
            );
        var newClient = Assert.IsType<Option<Client>.Result>(
            await _openidClients.GetClient(result.Item.ClientId, "token", TestContext.Current.CancellationToken)
        );

        Assert.Equal(2, newClient.Item.PostLogoutRedirectUris.Length);
        Assert.Single(newClient.Item.RedirectionUrls);
        Assert.Equal(2, newClient.Item.GrantTypes.Length);
    }

    [Fact]
    public async Task When_Delete_Client_Then_Ok_Is_Returned()
    {
        var addClientResult = Assert.IsType<Option<Client>.Result>(
            await _openidClients.AddClient(
                new Client
                {
                    ClientId = Guid.NewGuid().ToString("N"),
                    JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                    AllowedScopes = ["openid"],
                    ApplicationType = ApplicationTypes.Web,
                    ClientName = "client_name",
                    ClientUri = new Uri("http://clienturi.com"),
                    Contacts = ["contact"],
                    DefaultAcrValues = "sms",
                    //DefaultMaxAge = 10,
                    GrantTypes = [GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken],
                    RedirectionUrls = [new Uri("http://localhost")],
                    PostLogoutRedirectUris = [new Uri("http://localhost/callback")],
                    //LogoUri = new Uri("http://logouri.com")
                },
                "", TestContext.Current.CancellationToken)
        );

        var deleteResult =
            await _openidClients.DeleteClient(addClientResult.Item.ClientId, "token",
                TestContext.Current.CancellationToken);

        Assert.NotNull(deleteResult);
    }

    [Fact]
    public async Task When_Search_One_Client_Then_One_Client_Is_Returned()
    {
        _ = await _openidClients.AddClient(
            new Client
            {
                ClientId = Guid.NewGuid().ToString("N"),
                AllowedScopes = ["openid"],
                ApplicationType = ApplicationTypes.Web,
                ClientName = "client_name",
                IdTokenSignedResponseAlg = "RS256",
                IdTokenEncryptedResponseAlg = "RSA1_5",
                IdTokenEncryptedResponseEnc = "A128CBC-HS256",
                UserInfoSignedResponseAlg = "RS256",
                UserInfoEncryptedResponseAlg = "RSA1_5",
                UserInfoEncryptedResponseEnc = "A128CBC-HS256",
                RequestObjectSigningAlg = "RS256",
                RequestObjectEncryptionAlg = "RSA1_5",
                RequestObjectEncryptionEnc = "A128CBC-HS256",
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                InitiateLoginUri = new Uri("https://initloginuri"),
                ClientUri = new Uri("http://clienturi.com"),
                Contacts = ["contact"],
                DefaultAcrValues = "sms",
                //DefaultMaxAge = 10,
                GrantTypes = [GrantTypes.AuthorizationCode, GrantTypes.Implicit, GrantTypes.RefreshToken],
                ResponseTypes =
                [
                    ResponseTypeNames.Code, ResponseTypeNames.IdToken, ResponseTypeNames.Token
                ],
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                RedirectionUrls = [new Uri("http://localhost")],
                PostLogoutRedirectUris = [new Uri("http://localhost/callback")],
                //LogoUri = new Uri("http://logouri.com")
            },
            "", TestContext.Current.CancellationToken);

        var searchResult = Assert.IsType<Option<PagedResult<Client>>.Result>(
            await _openidClients.SearchClients(
                new SearchClientsRequest { StartIndex = 0, NbResults = 1 },
                "", TestContext.Current.CancellationToken)
        );

        Assert.Single(searchResult.Item.Content);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _server.Dispose();
    }
}
