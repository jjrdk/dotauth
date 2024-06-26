﻿namespace DotAuth.Server.Tests;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using Xunit.Abstractions;

public sealed class TokenFixture : IDisposable
{
    private const string BaseUrl = "http://localhost:5000";
    private const string WellKnownUma2Configuration = "/.well-known/uma2-configuration";
    private readonly UmaClient _umaClient;
    private readonly TestUmaServer _server;

    public TokenFixture(ITestOutputHelper outputHelper)
    {
        IdentityModelEventSource.ShowPII = true;
        _server = new TestUmaServer(outputHelper);
        _umaClient = new UmaClient(_server.Client, new Uri(BaseUrl + WellKnownUma2Configuration));
    }

    [Fact]
    public async Task When_Ticket_Id_Does_Not_Exist_Then_Error_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
            _server.Client,
            new Uri(BaseUrl + WellKnownUma2Configuration));
        // Try to get the access token via "ticket_id" grant-type.
        var token = Assert.IsType<Option<GrantedTokenResponse>.Error>(
            await tokenClient.GetToken(TokenRequest.FromTicketId("ticket_id", "")));

        Assert.Equal(ErrorCodes.InvalidGrant, token.Details.Title);
        Assert.Equal(string.Format(Strings.TheTicketDoesntExist, "ticket_id"), token.Details.Detail);
    }

    [Fact]
    public async Task When_Using_ClientCredentials_Grant_Type_Then_AccessToken_Is_Returned()
    {
        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
            _server.Client,
            new Uri(BaseUrl + WellKnownUma2Configuration));
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tokenClient
            .GetToken(TokenRequest.FromScopes("uma_protection", "uma_authorization")));

        Assert.NotEmpty(result.Item.AccessToken);
    }

    [Fact]
    public async Task When_Using_TicketId_Grant_Type_Then_AccessToken_Is_Returned()
    {
        var handler = new JwtSecurityTokenHandler();
        var set = new JsonWebKeySet();
        set.Keys.Add(_server.SharedUmaCtx.SignatureKey);

        var securityToken = new JwtSecurityToken(
            "http://server.example.com",
            "s6BhdRkqt3",
            new[] { new Claim("sub", "248289761001") },
            null,
            DateTime.UtcNow.AddYears(1),
            new SigningCredentials(set.GetSignKeys().First(), SecurityAlgorithms.HmacSha256));
        var jwt = handler.WriteToken(securityToken);

        var tc = new TokenClient(
            TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
            _server.Client,
            new Uri(BaseUrl + WellKnownUma2Configuration));
        // Get PAT.
        var result = Assert.IsType<Option<GrantedTokenResponse>.Result>(await tc
            .GetToken(TokenRequest.FromScopes("uma_protection", "uma_authorization")));

        var resourceSet = new ResourceSet
        {
            Name = "name",
            Scopes = new[] { "read", "write", "execute" },
            AuthorizationPolicies = new[]
            {
                new PolicyRule
                {
                    ClientIdsAllowed = new[] { "resource_server" },
                    Scopes = new[] { "read", "write", "execute" }
                }
            }
        };
        var resource = Assert.IsType<Option<AddResourceSetResponse>.Result>(
            await _umaClient.AddResourceSet(resourceSet, result.Item.AccessToken));
        resourceSet = resourceSet with { Id = resource.Item.Id };
        await _umaClient.UpdateResourceSet(resourceSet, result.Item.AccessToken);
        var ticket = Assert.IsType<Option<TicketResponse>.Result>(
            await _umaClient.RequestPermission(
                "header",
                requests: new PermissionRequest // Add permission & retrieve a ticket id.
                {
                    ResourceSetId = resource.Item.Id, Scopes = new[] { "read" }
                }));

        Assert.NotNull(ticket.Item);

        var tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
            _server.Client,
            new Uri(BaseUrl + WellKnownUma2Configuration));
        var option = await tokenClient
            .GetToken(TokenRequest.FromTicketId(ticket.Item.TicketId, jwt));
        var token = Assert.IsType<Option<GrantedTokenResponse>.Result>(option);

        var jwtToken = handler.ReadJwtToken(token.Item.AccessToken);
        Assert.NotNull(jwtToken.Claims);
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}
