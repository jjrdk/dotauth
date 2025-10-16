namespace DotAuth.Server.Tests;

using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

internal sealed class JwtBearerPostConfigureOptions : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly TestServer _server;

    public JwtBearerPostConfigureOptions(IServer server)
    {
        _server = server as TestServer ?? throw new ArgumentException(@"Expected TestServer", nameof(server));
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        options.Authority = _server.CreateClient().BaseAddress?.AbsoluteUri;
        options.BackchannelHttpHandler = _server.CreateHandler();
        options.RequireHttpsMetadata = false;
        options.Events = new JwtBearerEvents { OnAuthenticationFailed = ctx => throw ctx.Exception };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = false,
            SignatureValidator = (token, _) => new JsonWebToken(token),
            ValidIssuer = "http://localhost:5000"
        };
        if (string.IsNullOrEmpty(options.TokenValidationParameters.ValidAudience)
         && !string.IsNullOrEmpty(options.Audience))
        {
            options.TokenValidationParameters.ValidAudience = options.Audience;
        }

        if (options.ConfigurationManager != null)
        {
            return;
        }

        if (options.Configuration != null)
        {
            options.ConfigurationManager =
                new StaticConfigurationManager<OpenIdConnectConfiguration>(options.Configuration);
        }
        else if (!(string.IsNullOrEmpty(options.MetadataAddress) && string.IsNullOrEmpty(options.Authority)))
        {
            HandleMetadata(options);
        }
    }

    private static void HandleMetadata(JwtBearerOptions options)
    {
        if (string.IsNullOrEmpty(options.MetadataAddress) && !string.IsNullOrEmpty(options.Authority))
        {
            options.MetadataAddress = $"{options.Authority.TrimEnd('/')}/.well-known/openid-configuration";
        }

        if (options.RequireHttpsMetadata
         && !options.MetadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
        }
    }
}
