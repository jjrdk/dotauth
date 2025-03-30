namespace DotAuth.AcceptanceTests.Support;

using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

internal sealed class JwtBearerPostConfigureOptions : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly TestServer _server;

    public JwtBearerPostConfigureOptions(IServer server)
    {
        _server = (server as TestServer)!;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        options.Authority = _server.CreateClient().BaseAddress!.AbsoluteUri;
        options.BackchannelHttpHandler = _server.CreateHandler();
        options.RequireHttpsMetadata = false;
        options.Events = new JwtBearerEvents { OnAuthenticationFailed = ctx => throw ctx.Exception };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = false,
            ValidIssuer = "http://localhost",
//            IssuerSigningKeyResolver = (
//                string token,
//                SecurityToken securityToken,
//                string kid,
//                TokenValidationParameters validationParameters) =>
//            {
//                var metadataAddress = options.MetadataAddress;
//                var client = new HttpClient(options.BackchannelHttpHandler);
//                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
//                    metadataAddress,
//                    new OpenIdConnectConfigurationRetriever(),
//                    new HttpDocumentRetriever(client) { RequireHttps = options.RequireHttpsMetadata });
//                var config = configurationManager.GetConfigurationAsync().GetAwaiter().GetResult();
//                return config.SigningKeys;
//            }
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

//            var httpClient = new HttpClient(options.BackchannelHttpHandler ?? _server.CreateHandler())
//            {
//                Timeout = options.BackchannelTimeout,
//                MaxResponseContentBufferSize = 1024 * 1024 * 10 // 10 MB
//            };
//
//            options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
//                options.MetadataAddress,
//                new TestConfigurationRetriever(),
//                new HttpDocumentRetriever(httpClient) { RequireHttps = options.RequireHttpsMetadata });
        }
    }
}
//
//internal class TestConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
//{
//    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
//        string address,
//        IDocumentRetriever retriever,
//        CancellationToken cancel)
//    {
//        string json1 = await retriever.GetDocumentAsync(address, cancel).ConfigureAwait(false);
//        OpenIdConnectConfiguration
//            openIdConnectConfiguration =
//                JsonSerializer
//                    .Deserialize<OpenIdConnectConfiguration>(json1); //OpenIdConnectConfigurationSerializer.Read(json1);
//        var result = await OpenIdConnectConfigurationRetriever.GetAsync(address, retriever, cancel);
//        return result;
//    }
//}
