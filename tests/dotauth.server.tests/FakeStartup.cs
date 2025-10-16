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

namespace DotAuth.Server.Tests;

using System.Net.Http;
using System.Security.Cryptography;
using DotAuth;
using DotAuth.Repositories;
using DotAuth.Server.Tests.MiddleWares;
using DotAuth.Server.Tests.Stores;
using DotAuth.Services;
using DotAuth.Shared.Repositories;
using DotAuth.Sms;
using DotAuth.Sms.Services;
using DotAuth.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using NSubstitute;
using Xunit.Abstractions;

public sealed class FakeStartup
{
    private readonly SharedContext _context;
    private readonly ITestOutputHelper _testOutputHelper;

    public const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;

    public FakeStartup(SharedContext context, ITestOutputHelper testOutputHelper)
    {
        _context = context;
        _testOutputHelper = testOutputHelper;
        IdentityModelEventSource.ShowPII = true;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddCors(options =>
            options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = DefaultSchema;
                opts.DefaultChallengeScheme = DefaultSchema;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, _ => { })
            .AddFakeCustomAuth(_ => { });

        services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>()
            .AddDotAuthServer(options =>
                {
                    options.DataProtector = _ => new SymmetricDataProtector(CreateSymmetricAlgorithm());
                    options.AdministratorRoleDefinition = default;
                    options.Clients = sp => new InMemoryClientRepository(
                        sp.GetRequiredService<IHttpClientFactory>(),
                        sp.GetRequiredService<IScopeStore>(),
                        Substitute.For<ILogger<InMemoryClientRepository>>(),
                        DefaultStores.Clients(_context));
                    options.Consents = _ => new InMemoryConsentRepository(DefaultStores.Consents());
                    options.Users = sp => new InMemoryResourceOwnerRepository(string.Empty, DefaultStores.Users());
                },
                [JwtBearerDefaults.AuthenticationScheme])
            .AddSmsAuthentication(_context.TwilioClient)
            .AddLogging(b => b.AddXunit(_testOutputHelper))
            .AddAccountFilter()
            .AddSingleton(_context.ConfirmationCodeStore)
            .AddSingleton(sp =>
            {
                var server = sp.GetRequiredService<IServer>() as TestServer;
                return server!.CreateClient();
            });
        services.ConfigureOptions<JwtBearerPostConfigureOptions>();
    }

    private static Aes CreateSymmetricAlgorithm()
    {
        var symmetricAlgorithm = Aes.Create();
        symmetricAlgorithm.GenerateIV();
        symmetricAlgorithm.GenerateKey();
        return symmetricAlgorithm;
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseDotAuthServer(applicationTypes: typeof(IDefaultUi));
    }
}
