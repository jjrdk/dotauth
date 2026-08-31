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

using DotAuth.Mcp.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Authentication ---
// Align with the existing DotAuth JWT bearer pattern (see src/dotauth.authserver/Startup.cs).
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(cfg =>
    {
        cfg.Authority = builder.Configuration["OAUTH:AUTHORITY"];
        cfg.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuers = (builder.Configuration["OAUTH:VALIDISSUERS"] ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray()
        };
        // Local/test environments may not serve HTTPS metadata.
        cfg.RequireHttpsMetadata = false;
    });

// --- Authorization ---
// Mirrors the "manager" policy from src/dotauth/ServiceCollectionExtensions.cs:
// the caller must present a bearer token that contains the "manager" scope.
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
                        .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Any(s => s == "manager");
            });
        });
});

// --- MCP server with HTTP (SSE) transport ---
// Tools are resolved from DI so the consuming host must register the repository
// implementations it wants to back the tool surface (e.g. IClientStore, IScopeStore,
// IResourceOwnerStore).
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<ClientTools>()
    .WithTools<ScopeTools>()
    .WithTools<UserTools>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// The /mcp endpoint is only reachable by callers with a valid "manager"-scoped bearer token.
app.MapMcp("/mcp").RequireAuthorization("manager");

app.Run();

