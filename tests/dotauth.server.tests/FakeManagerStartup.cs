﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

using System.Reflection;
using DotAuth;
using DotAuth.Controllers;
using DotAuth.Repositories;
using DotAuth.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public sealed class FakeManagerStartup
{
    private const string DefaultSchema = "Cookies";

    public void ConfigureServices(IServiceCollection services)
    {
        RegisterServices(services);
        services.AddControllers().AddApplicationPart(typeof(ClientsController).GetTypeInfo().Assembly);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseDotAuthServer(applicationTypes: typeof(IDefaultUi));
    }

    private static void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();
        serviceCollection.AddDotAuthServer(
                new DotAuthConfiguration
                {
                    Users = _ => new InMemoryResourceOwnerRepository(string.Empty, DefaultStorage.GetUsers()),
                },
                new[] { JwtBearerDefaults.AuthenticationScheme })
            .AddDotAuthUi(typeof(IDefaultUi));
        serviceCollection.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = DefaultSchema;
            opts.DefaultChallengeScheme = DefaultSchema;
        });
        serviceCollection.AddAuthorization(options =>
        {
            options.AddPolicy("manager", policy => { policy.RequireAssertion(_ => true); });
        });
    }
}
