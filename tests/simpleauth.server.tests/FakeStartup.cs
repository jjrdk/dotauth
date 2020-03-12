// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Server.Tests
{
    using System;
    using Extensions;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Repositories;
    using Stores;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Moq;
    using SimpleAuth.Server.Tests.MiddleWares;
    using SimpleAuth.Shared;
    using SimpleAuth.Sms;
    using SimpleAuth.Sms.Services;

    public class FakeStartup
    {
        private readonly SharedContext _context;
        public const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;

        public FakeStartup(SharedContext context)
        {
            _context = context;
            IdentityModelEventSource.ShowPII = true;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.AddAuthentication(
                    opts =>
                    {
                        opts.DefaultAuthenticateScheme = DefaultSchema;
                        opts.DefaultChallengeScheme = DefaultSchema;
                    })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, cfg => { })
                .AddFakeCustomAuth(o => { });

            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>()
                .AddSimpleAuth(
                    options =>
                    {
                        options.Clients = sp => new InMemoryClientRepository(
                            sp.GetRequiredService<HttpClient>(),
                            sp.GetService<IScopeStore>(),
                            new Mock<ILogger<InMemoryClientRepository>>().Object,
                            DefaultStores.Clients(_context));
                        options.Consents = sp => new InMemoryConsentRepository(DefaultStores.Consents());
                        options.Users = sp => new InMemoryResourceOwnerRepository(DefaultStores.Users());
                    },
                    new[] { JwtBearerDefaults.AuthenticationScheme })
                .AddSmsAuthentication(_context.TwilioClient.Object)
                .AddLogging()
                .AddAccountFilter()
                .AddSingleton(_context.ConfirmationCodeStore.Object)
                .AddSingleton(
                    sp =>
                    {
                        var server = sp.GetRequiredService<IServer>() as TestServer;
                        return server.CreateClient();
                    });
            services.ConfigureOptions<JwtBearerPostConfigureOptions>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSimpleAuthMvc();
        }
    }

    internal class JwtBearerPostConfigureOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly TestServer _server;

        public JwtBearerPostConfigureOptions(IServer server)
        {
            _server = server as TestServer;
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            options.Authority = _server.CreateClient().BaseAddress.AbsoluteUri;
            options.BackchannelHttpHandler = _server.CreateHandler();
            options.RequireHttpsMetadata = false;
            options.Events = new JwtBearerEvents { OnAuthenticationFailed = ctx => throw ctx.Exception };
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = false,
                ValidIssuer = "http://localhost:5000"
            };
            if (string.IsNullOrEmpty(options.TokenValidationParameters.ValidAudience)
                && !string.IsNullOrEmpty(options.Audience))
            {
                options.TokenValidationParameters.ValidAudience = options.Audience;
            }

            if (options.ConfigurationManager == null)
            {
                if (options.Configuration != null)
                {
                    options.ConfigurationManager =
                        new StaticConfigurationManager<OpenIdConnectConfiguration>(options.Configuration);
                }
                else if (!(string.IsNullOrEmpty(options.MetadataAddress) && string.IsNullOrEmpty(options.Authority)))
                {
                    if (string.IsNullOrEmpty(options.MetadataAddress) && !string.IsNullOrEmpty(options.Authority))
                    {
                        options.MetadataAddress = options.Authority;
                        if (!options.MetadataAddress.EndsWith("/", StringComparison.Ordinal))
                        {
                            options.MetadataAddress += "/";
                        }

                        options.MetadataAddress += ".well-known/openid-configuration";
                    }

                    if (options.RequireHttpsMetadata
                        && !options.MetadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "The MetadataAddress or Authority must use HTTPS unless disabled for development by setting RequireHttpsMetadata=false.");
                    }

                    var httpClient = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler())
                    {
                        Timeout = options.BackchannelTimeout,
                        MaxResponseContentBufferSize = 1024 * 1024 * 10 // 10 MB
                    };

                    options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        options.MetadataAddress,
                        new OpenIdConnectConfigurationRetriever(),
                        new HttpDocumentRetriever(httpClient) { RequireHttps = options.RequireHttpsMetadata });
                }
            }
        }
    }
}
