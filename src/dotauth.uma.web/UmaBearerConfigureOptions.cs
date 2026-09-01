namespace DotAuth.Uma.Web;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

internal sealed class UmaBearerConfigureOptions : IConfigureNamedOptions<UmaBearerOptions>
{
    private readonly IAuthenticationConfigurationProvider _authenticationConfigurationProvider;
    private static readonly Func<string, TimeSpan> InvariantTimeSpanParse = (string timespanString) => TimeSpan.Parse(timespanString, CultureInfo.InvariantCulture);

    /// <summary>
    /// Initializes a new <see cref="UmaBearerConfigureOptions"/> given the configuration
    /// provided by the <paramref name="configurationProvider"/>.
    /// </summary>
    /// <param name="configurationProvider">An <see cref="IAuthenticationConfigurationProvider"/> instance.</param>
    public UmaBearerConfigureOptions(IAuthenticationConfigurationProvider configurationProvider)
    {
        _authenticationConfigurationProvider = configurationProvider;
    }

    /// <inheritdoc />
    public void Configure(string? name, UmaBearerOptions options)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        var configSection = _authenticationConfigurationProvider.GetSchemeConfiguration(name);

        if (!configSection.GetChildren().Any())
        {
            return;
        }

        var validateIssuer = ParseValueOrDefault(configSection[nameof(TokenValidationParameters.ValidateIssuer)], bool.Parse, options.TokenValidationParameters.ValidateIssuer);
        var issuer = configSection[nameof(TokenValidationParameters.ValidIssuer)];
        var issuers = configSection.GetSection(nameof(TokenValidationParameters.ValidIssuers)).GetChildren().Select(iss => iss.Value).ToList();
        var validateAudience = ParseValueOrDefault(configSection[nameof(TokenValidationParameters.ValidateAudience)], bool.Parse, options.TokenValidationParameters.ValidateAudience);
        var audience = configSection[nameof(TokenValidationParameters.ValidAudience)];
        var audiences = configSection.GetSection(nameof(TokenValidationParameters.ValidAudiences)).GetChildren().Select(aud => aud.Value).ToList();

        options.Authority = configSection[nameof(options.Authority)] ?? options.Authority;
        options.BackchannelTimeout = ParseValueOrDefault(configSection[nameof(options.BackchannelTimeout)], InvariantTimeSpanParse, options.BackchannelTimeout);
        options.Challenge = configSection[nameof(options.Challenge)] ?? options.Challenge;
        options.ForwardAuthenticate = configSection[nameof(options.ForwardAuthenticate)] ?? options.ForwardAuthenticate;
        options.ForwardChallenge = configSection[nameof(options.ForwardChallenge)] ?? options.ForwardChallenge;
        options.ForwardDefault = configSection[nameof(options.ForwardDefault)] ?? options.ForwardDefault;
        options.ForwardForbid = configSection[nameof(options.ForwardForbid)] ?? options.ForwardForbid;
        options.ForwardSignIn = configSection[nameof(options.ForwardSignIn)] ?? options.ForwardSignIn;
        options.ForwardSignOut = configSection[nameof(options.ForwardSignOut)] ?? options.ForwardSignOut;
        options.IncludeErrorDetails = ParseValueOrDefault(configSection[nameof(options.IncludeErrorDetails)], bool.Parse, options.IncludeErrorDetails);
        options.MapInboundClaims = ParseValueOrDefault( configSection[nameof(options.MapInboundClaims)], bool.Parse, options.MapInboundClaims);
        options.MetadataAddress = configSection[nameof(options.MetadataAddress)] ?? options.MetadataAddress;
        options.RefreshInterval = ParseValueOrDefault(configSection[nameof(options.RefreshInterval)], InvariantTimeSpanParse, options.RefreshInterval);
        options.RefreshOnIssuerKeyNotFound = ParseValueOrDefault(configSection[nameof(options.RefreshOnIssuerKeyNotFound)], bool.Parse, options.RefreshOnIssuerKeyNotFound);
        options.RequireHttpsMetadata = ParseValueOrDefault(configSection[nameof(options.RequireHttpsMetadata)], bool.Parse, options.RequireHttpsMetadata);
        options.SaveToken = ParseValueOrDefault(configSection[nameof(options.SaveToken)], bool.Parse, options.SaveToken);
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = validateIssuer,
            ValidIssuers = issuers,
            ValidIssuer = issuer,
            ValidateAudience = validateAudience,
            ValidAudiences = audiences,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = GetIssuerSigningKeys(configSection, [issuer, ..issuers]),
        };
    }

    private static T ParseValueOrDefault<T>(string? value, Func<string, T> parseFunc, T defaultValue)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        try
        {
            return parseFunc(value);
        }
        catch
        {
            return defaultValue;
        }
    }

    private static IEnumerable<SecurityKey> GetIssuerSigningKeys(IConfiguration configuration, List<string?> issuers)
    {
        foreach (var issuer in issuers)
        {
            var signingKey = configuration.GetSection("SigningKeys")
                .GetChildren()
                .SingleOrDefault(key => key["Issuer"] == issuer);
            if (signingKey?["Value"] is {} keyValue)
            {
                yield return new SymmetricSecurityKey(Convert.FromBase64String(keyValue));
            }
        }
    }

    /// <inheritdoc />
    public void Configure(UmaBearerOptions options)
    {
        Configure(Options.DefaultName, options);
    }
}
