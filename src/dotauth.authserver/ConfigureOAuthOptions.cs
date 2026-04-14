namespace DotAuth.AuthServer;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

internal sealed class ConfigureOAuthOptions : IPostConfigureOptions<OAuthOptions>
{
    private readonly IConfiguration _configuration;

    public ConfigureOAuthOptions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, OAuthOptions options)
    {
        var authority = ResolveAuthority(_configuration);
        options.AuthorizationEndpoint = $"{authority}/authorization";
        options.TokenEndpoint = $"{authority}/token";
        options.UserInformationEndpoint = $"{authority}/userinfo";

        options.UsePkce = true;
        options.CallbackPath = "/callback";
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = ctx =>
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(ctx.AccessToken);
                ctx.Identity!.AddClaims(jwt.Claims.Where(c => !ctx.Identity.HasClaim(x => x.Type == c.Type)));
                ctx.Success();
                return Task.CompletedTask;
            }
        };
        options.SaveTokens = true;
        options.ClientId = "web";
        options.ClientSecret = "secret";
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("manager");
        options.Scope.Add("uma_protection");
    }

    private static string ResolveAuthority(IConfiguration configuration)
    {
        var configuredAuthority = configuration["OAUTH:AUTHORITY"] ?? configuration["OAuth:Authority"];
        if (TryNormalizeAbsoluteUri(configuredAuthority, out var authority))
        {
            return authority;
        }

        var configuredUrls = configuration["URLS"] ?? configuration["Urls"];
        if (!string.IsNullOrWhiteSpace(configuredUrls))
        {
            foreach (var candidate in configuredUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (TryNormalizeAbsoluteUri(candidate, out authority))
                {
                    return authority;
                }
            }
        }

        throw new InvalidOperationException("No absolute OAuth authority could be resolved. Configure 'OAUTH:AUTHORITY' or an absolute 'Urls' value.");
    }

    private static bool TryNormalizeAbsoluteUri(string? candidate, out string authority)
    {
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            authority = uri.AbsoluteUri.TrimEnd('/');
            return true;
        }

        authority = string.Empty;
        return false;
    }
}
