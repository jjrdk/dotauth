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
    using Controllers;
    using Extensions;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using MiddleWares;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Repositories;
    using Stores;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Moq;
    using SimpleAuth.Shared;
    using SimpleAuth.Sms.Controllers;
    using SimpleAuth.Sms.Services;

    public class FakeStartup : IStartup
    {
        private const string Bearer = "Bearer ";
        private static readonly int StartIndex = Bearer.Length;
        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();
        public const string DefaultSchema = CookieAuthenticationDefaults.AuthenticationScheme;
        private readonly SharedContext _context;

        public FakeStartup(SharedContext context)
        {
            IdentityModelEventSource.ShowPII = true;
            _context = context;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // 1. Add the dependencies needed to enable CORS
            services.AddCors(
                options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            // 2. Configure server
            ConfigureIdServer(services);
            services.AddAuthentication(
                    opts =>
                    {
                        opts.DefaultAuthenticateScheme = DefaultSchema;
                        opts.DefaultChallengeScheme = DefaultSchema;
                    })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    {
                        cfg.RequireHttpsMetadata = false;
                        cfg.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = ctx =>
                            {
                                string authorization = ctx.Request.Headers["Authorization"];
                                if (string.IsNullOrWhiteSpace(authorization))
                                {
                                    ctx.NoResult();
                                    return Task.CompletedTask;
                                }

                                string token = null;
                                if (authorization.StartsWith(Bearer, StringComparison.OrdinalIgnoreCase))
                                {
                                    token = authorization.Substring(StartIndex).Trim();
                                }

                                var jwt = (JwtSecurityToken) _handler.ReadToken(token);
                                var claimsIdentity = new ClaimsIdentity(
                                    jwt.Claims,
                                    JwtBearerDefaults.AuthenticationScheme);
                                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                                ctx.Principal = claimsPrincipal;
                                ctx.Success();
                                return Task.CompletedTask;
                            }
                        };
                        cfg.TokenValidationParameters = new TokenValidationParameters {ValidateAudience = false,};
                    })
                .AddFakeCustomAuth(o => { });

            services.AddAuthorization(
                opt => { opt.AddAuthPolicies(DefaultSchema, JwtBearerDefaults.AuthenticationScheme); });
            // 3. Configure MVC
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(DiscoveryController).Assembly));
            parts.Add(new AssemblyPart(typeof(CodeController).Assembly));

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            //1 . Enable CORS.
            app.UseCors("AllowAll");
            app.UseSimpleAuthExceptionHandler();
            //// 5. Client JWKS endpoint
            //app.Map("/jwks_client", a =>
            //{
            //    a.Run(async ctx =>
            //    {
            //        var jwks = new[]
            //        {
            //            _context.EncryptionKey,
            //            _context.SignatureKey
            //        };
            //        var repo = app.ApplicationServices.GetService<IJwksActions>();
            //        var result = await repo.GetJwks().ConfigureAwait(false);

            //        var json = JsonConvert.SerializeObject(result);
            //        var data = Encoding.UTF8.GetBytes(json);
            //        ctx.Response.ContentType = "application/json";
            //        await ctx.Response.Body.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            //    });
            //});
            // 5. Use MVC.
            app.UseMvc();
        }

        private void ConfigureIdServer(IServiceCollection services)
        {
            services.AddSingleton(_context.TwilioClient.Object)
                .AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>()
                .AddSimpleAuth(
                    options =>
                    {
                        options.Clients = sp => new InMemoryClientRepository(
                            _context.Client,
                            sp.GetService<IScopeStore>(),
                            new Mock<ILogger<InMemoryClientRepository>>().Object,
                            DefaultStores.Clients(_context));
                        options.Consents = sp => new InMemoryConsentRepository(DefaultStores.Consents());
                        options.Users = sp => new InMemoryResourceOwnerRepository(DefaultStores.Users());
                    })
                .AddLogging()
                .AddAccountFilter()
                .AddSingleton(_context.ConfirmationCodeStore.Object)
                .AddSingleton(sp => _context.Client);
        }
    }
}
