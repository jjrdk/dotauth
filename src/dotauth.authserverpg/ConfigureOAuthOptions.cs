namespace DotAuth.AuthServerPg;

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotAuth.Shared;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal sealed partial class ConfigureOAuthOptions : IPostConfigureOptions<OAuthOptions>
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
            LogSelfSignedCertsAllowed();
            options.Backchannel = new HttpClient(
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (msg, cert, _, _) =>
                    {
                        var altNames = cert!.GetSubjectAlternativeNames();
                        LogSubjectAltNamesAltnames(string.Join(", ", altNames));
                        var requestUriHost = msg.RequestUri?.Host;
                        LogRequestHostHost(requestUriHost);
                        var allowed = requestUriHost != null &&
                            (altNames.Count == 0 || altNames.Contains(requestUriHost));
                        if (!allowed)
                        {
                            LogCertificateWithThumbprintThumbprintNotAllowed(cert!.Thumbprint);
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
            LogDefaultCertificateValidation();
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
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<ConfigureOAuthOptions>>();
                logger.LogError(ctx.Failure, "{Error}", ctx.Failure!.Message);
                return Task.CompletedTask;
            },
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

    [LoggerMessage(LogLevel.Warning, "Self signed certs allowed")]
    partial void LogSelfSignedCertsAllowed();

    [LoggerMessage(LogLevel.Information, "Subject alt names: {AltNames}")]
    partial void LogSubjectAltNamesAltnames(string altNames);

    [LoggerMessage(LogLevel.Information, "Request host: {Host}")]
    partial void LogRequestHostHost(string? host);

    [LoggerMessage(LogLevel.Warning, "Certificate with thumbprint {Thumbprint} not allowed")]
    partial void LogCertificateWithThumbprintThumbprintNotAllowed(string thumbprint);

    [LoggerMessage(LogLevel.Information, "Default certificate validation")]
    partial void LogDefaultCertificateValidation();
}
