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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using DotAuth;
using DotAuth.Repositories;
using DotAuth.Server.Tests.MiddleWares;
using DotAuth.Server.Tests.Stores;
using DotAuth.Shared;
using DotAuth.Shared.Policies;
using DotAuth.Shared.Repositories;
using DotAuth.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

public sealed class FakeUmaStartup
{
    private readonly ITestOutputHelper _outputHelper;
    public const string DefaultSchema = "OAuth2Introspection";

    public FakeUmaStartup(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // 1. Add the dependencies.
        // 2. Add authorization policies.
        services.AddAuthentication(
                opts =>
                {
                    opts.DefaultAuthenticateScheme = DefaultSchema;
                    opts.DefaultChallengeScheme = DefaultSchema;
                })
            .AddUmaCustomAuth(_ => { });
        services.AddAuthorization(
            opts =>
            {
                opts.AddAuthPolicies((OpenIdClaimTypes.Role, "administrator"), DefaultSchema)
                    .AddPolicy(
                        "UmaProtection",
                        policy =>
                        {
                            policy.AddAuthenticationSchemes(DefaultSchema);
                            policy.RequireAssertion(_ => true);
                        });
            });
        // 3. Add the dependencies needed to enable CORS
        services.AddCors(
            options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation()
            .AddApplicationPart(typeof(CoreConstants).Assembly);
        services.AddRazorPages();
        services.AddDotAuthServer(
            new DotAuthConfiguration
            {
                Clients = sp => new InMemoryClientRepository(
                    new Mock<IHttpClientFactory>().Object,
                    sp.GetService<IScopeStore>(),
                    new Mock<ILogger<InMemoryClientRepository>>().Object,
                    OAuthStores.GetClients()),
                Scopes = _ => new InMemoryScopeRepository(OAuthStores.GetScopes()),
                ResourceSets = sp => new InMemoryResourceSetRepository(sp.GetRequiredService<IAuthorizationPolicy>(), UmaStores.GetResources())
            },
            new[] { DefaultSchema },
            assemblyTypes: typeof(IDefaultUi));

        // 3. Enable logging.
        services.AddLogging(l => l.AddXunit(_outputHelper));
        // 5. Register other classes.
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    }

    public void Configure(IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        app.Use(
            async (context, next) =>
            {
                var claimsIdentity = new ClaimsIdentity(
                    new List<Claim>
                    {
                        new("client_id", "resource_server"),
                        new("sub", "resource_server")
                    },
                    "fakests");
                context.User = new ClaimsPrincipal(claimsIdentity);
                await next.Invoke().ConfigureAwait(false);
            });

        app.UseDotAuthServer(applicationTypes: typeof(IDefaultUi));
    }
}