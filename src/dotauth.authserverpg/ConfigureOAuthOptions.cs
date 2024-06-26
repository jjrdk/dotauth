﻿namespace DotAuth.AuthServerPg;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotAuth.Shared;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal sealed class ConfigureOAuthOptions : IPostConfigureOptions<OAuthOptions>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigureOAuthOptions> _logger;

    public ConfigureOAuthOptions(IConfiguration configuration, ILogger<ConfigureOAuthOptions> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, OAuthOptions options)
    {
        var authority = _configuration["OAUTH:AUTHORITY"];
        options.AuthorizationEndpoint = $"{authority}/authorization";
        options.TokenEndpoint = $"{authority}/token";
        options.UserInformationEndpoint = $"{authority}/userinfo";
        options.UsePkce = true;
        options.CallbackPath = "/callback";
        if (bool.TryParse(_configuration["SERVER:ALLOWSELFSIGNEDCERT"], out var allowSelfSignedCert)
            && allowSelfSignedCert)
        {
            _logger.LogWarning("Self signed certs allowed");
            options.Backchannel = new HttpClient(
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (msg, cert, _, _) =>
                    {
                        var altNames = cert!.GetSubjectAlternativeNames();
                        _logger.LogInformation("Subject alt names: {altNames}", string.Join(", ", altNames));
                        var requestUriHost = msg.RequestUri?.Host;
                        _logger.LogInformation("Request host: {host}", requestUriHost);
                        var allowed = requestUriHost != null && (altNames.Count == 0 || altNames.Contains(requestUriHost));
                        if (!allowed)
                        {
                            _logger.LogWarning("Certificate with thumbprint {thumbprint} not allowed", cert!.Thumbprint);
                        }

                        return allowed;
                    },
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.All
                },
                true);
        }
        else
        {
            _logger.LogInformation("Default certificate validation");
        }

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = ctx =>
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(ctx.AccessToken);
                var claims = jwt.Claims.Where(c => !ctx.Identity!.HasClaim(x => x.Type == c.Type)).ToArray();
                ctx.Identity!.AddClaims(claims);
                ctx.Success();
                return Task.CompletedTask;
            },
            OnRemoteFailure = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<IApplicationBuilder>>();
                logger.LogError(ctx.Failure, "{error}", ctx.Failure!.Message);
                return Task.CompletedTask;
            }
        };
        options.SaveTokens = true;
        options.ClientId = _configuration["OAUTH:CLIENTID"] ?? "";
        options.ClientSecret = _configuration["OAUTH:CLIENTSECRET"] ?? "";
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("manager");
        options.Scope.Add("uma_protection");
    }
}
