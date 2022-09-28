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

namespace SimpleAuth.Server.Tests;

using Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth;
using SimpleAuth.Repositories;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SimpleAuth.UI;

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
        app.UseSimpleAuthMvc(applicationTypes: typeof(IDefaultUi));
    }

    private static void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();
        serviceCollection.AddSimpleAuth(
            new SimpleAuthOptions
            {
                Users = sp => new InMemoryResourceOwnerRepository(string.Empty, DefaultStorage.GetUsers()),
            },
            new[] {JwtBearerDefaults.AuthenticationScheme},
            assemblyTypes: typeof(IDefaultUi));
        serviceCollection.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = DefaultSchema;
            opts.DefaultChallengeScheme = DefaultSchema;
        });
        serviceCollection.AddAuthorization(options =>
        {
            options.AddPolicy("manager", policy =>
            {
                policy.RequireAssertion(p => true);
            });
        });
    }
}