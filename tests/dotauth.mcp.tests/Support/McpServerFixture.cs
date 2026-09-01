// Copyright © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace DotAuth.Mcp.Tests.Support;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using dotauth.mcp.Tools;
using DotAuth.Shared.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

/// <summary>
/// Builds an in-process <see cref="TestServer"/> hosting the MCP server with stub repositories.
/// JWT signature validation is disabled so tests can manufacture tokens freely.
/// One fixture instance is created per Reqnroll scenario.
/// </summary>
public sealed class McpServerFixture : IDisposable
{
    public TestServer Server { get; }

    /// <summary>Stub client store — configure per-scenario via NSubstitute.</summary>
    public IClientStore ClientStore { get; } = Substitute.For<IClientStore>();

    /// <summary>Stub scope store — configure per-scenario via NSubstitute.</summary>
    public IScopeStore ScopeStore { get; } = Substitute.For<IScopeStore>();

    /// <summary>Stub user store — configure per-scenario via NSubstitute.</summary>
    public IResourceOwnerStore UserStore { get; } = Substitute.For<IResourceOwnerStore>();

    public McpServerFixture()
    {
        // Capture stub references for closure below.
        var clientStore = ClientStore;
        var scopeStore = ScopeStore;
        var userStore = UserStore;

        // Use WebApplicationBuilder so routing/MCP services are registered automatically.
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        // JWT bearer auth — signature validation is bypassed for unit tests.
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = false,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = false,
                    SignatureValidator = (token, _) => new JsonWebToken(token)
                };
            });

        builder.Services.AddAuthorization(opts =>
        {
            opts.AddPolicy(
                "manager",
                policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(ctx =>
                    {
                        var scopeClaim = ctx.User.FindFirst("scope");
                        return scopeClaim is not null
                            && scopeClaim.Value
                                .Split(
                                    ' ',
                                    StringSplitOptions.TrimEntries
                                    | StringSplitOptions.RemoveEmptyEntries)
                                .Any(s => s == "manager");
                    });
                });
        });

        // MCP server with the three tool types wired to stub stores.
        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithTools<ClientTools>()
            .WithTools<ScopeTools>()
            .WithTools<UserTools>();

        // Register stub stores as the backing implementations.
        builder.Services.AddSingleton(clientStore);
        builder.Services.AddSingleton(scopeStore);
        builder.Services.AddSingleton(userStore);

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapMcp("/mcp").RequireAuthorization("manager");

        app.StartAsync().GetAwaiter().GetResult();
        Server = app.GetTestServer();
    }

    /// <summary>
    /// Creates a minimal JWT bearer token with the given scope claim value.
    /// The token is unsigned (signature validation is disabled in the fixture).
    /// </summary>
    public static string CreateBearerToken(string scope)
    {
        var handler = new JwtSecurityTokenHandler { SetDefaultTimesOnTokenCreation = false };
        var token = handler.CreateJwtSecurityToken(
            issuer: "test",
            subject: new ClaimsIdentity(new List<Claim>
            {
                new("sub", "test-user"),
                new("scope", scope)
            }));

        return handler.WriteToken(token);
    }

    public void Dispose()
    {
        Server.Dispose();
    }
}
