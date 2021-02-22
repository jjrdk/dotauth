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
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;

    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Repositories;

    using Stores;
    using System.Net.Http;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Logging;
    using Moq;

    using SimpleAuth.Server.Tests.MiddleWares;
    using SimpleAuth.Services;
    using SimpleAuth.Sms;
    using SimpleAuth.Sms.Services;
    using SimpleAuth.UI;

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
            services.AddHttpClient();
            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.AddAuthentication(
                    opts =>
                    {
                        opts.DefaultAuthenticateScheme = DefaultSchema;
                        opts.DefaultChallengeScheme = DefaultSchema;
                    })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, _ => { })
                .AddFakeCustomAuth(_ => { });

            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>()
                .AddSimpleAuth(
                    options =>
                    {
                        options.AdministratorRoleDefinition = default;
                        options.Clients = sp => new InMemoryClientRepository(
                            sp.GetRequiredService<IHttpClientFactory>(),
                            sp.GetService<IScopeStore>(),
                            new Mock<ILogger<InMemoryClientRepository>>().Object,
                            DefaultStores.Clients(_context));
                        options.Consents = _ => new InMemoryConsentRepository(DefaultStores.Consents());
                        options.Users = sp => new InMemoryResourceOwnerRepository(string.Empty, DefaultStores.Users());
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
            app.UseSimpleAuthMvc(applicationTypes: typeof(IDefaultUi));
        }
    }
}
