// Copyright 2015 Habart Thierry
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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SimpleIdentityServer.Authenticate.SMS;
using SimpleIdentityServer.Authenticate.SMS.Actions;
using SimpleIdentityServer.Authenticate.SMS.Controllers;
using SimpleIdentityServer.Authenticate.SMS.Services;
using SimpleIdentityServer.Core;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Host.Tests.MiddleWares;
using SimpleIdentityServer.Host.Tests.Services;
using SimpleIdentityServer.Host.Tests.Stores;
using SimpleIdentityServer.Logging;
using SimpleIdentityServer.Store;
using SimpleIdentityServer.Twilio.Client;
using SimpleIdentityServer.UserManagement.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleIdentityServer.Host.Tests
{
    using Controllers.Api;
    using Extensions;
    using System.Net.Http;
    using Core.Common.Repositories;

    public class FakeStartup : IStartup
    {
        public const string ScimEndPoint = "http://localhost:5555/";
        public const string DefaultSchema = "Cookies";
        private readonly IdentityServerOptions _options;
        private readonly IJsonWebKeyEnricher _jsonWebKeyEnricher;
        private readonly SharedContext _context;

        public FakeStartup(SharedContext context)
        {
            _options = new IdentityServerOptions
            {
                Scim = new ScimOptions
                {
                    IsEnabled = true,
                    EndPoint = ScimEndPoint
                }
            };
            _jsonWebKeyEnricher = new JsonWebKeyEnricher();
            _context = context;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // 1. Add the dependencies needed to enable CORS
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));
            // 2. Configure Simple identity server
            ConfigureIdServer(services);
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = DefaultSchema;
                opts.DefaultChallengeScheme = DefaultSchema;
            })
            .AddFakeCustomAuth(o => { })
            .AddFakeOAuth2Introspection(o =>
            {
                o.WellKnownConfigurationUrl = "http://localhost:5000/.well-known/openid-configuration";
                o.ClientId = "stateless_client";
                o.ClientSecret = "stateless_client";
                o.Client = _context.Client;
                // o.IdentityServerClientFactory = new IdentityServerClientFactory(_context.Oauth2IntrospectionHttpClientFactory.Object);
            })
            .AddFakeUserInfoIntrospection(o => { });
            services.AddAuthorization(opt =>
            {
                opt.AddOpenIdSecurityPolicy(DefaultSchema);
            });
            // 3. Configure MVC
            var mvc = services.AddMvc();
            var parts = mvc.PartManager.ApplicationParts;
            parts.Clear();
            parts.Add(new AssemblyPart(typeof(DiscoveryController).GetTypeInfo().Assembly));
            parts.Add(new AssemblyPart(typeof(CodeController).GetTypeInfo().Assembly));
            parts.Add(new AssemblyPart(typeof(ProfilesController).GetTypeInfo().Assembly));
            //parts.Add(new AssemblyPart(typeof(FiltersController).GetTypeInfo().Assembly));
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {

            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 4. Use simple identity server.
            app.UseOpenIdApi(_options);
            // 5. Client JWKS endpoint
            app.Map("/jwks_client", a =>
            {
                a.Run(async ctx =>
                {
                    var jwks = new[]
                    {
                        _context.EncryptionKey,
                        _context.SignatureKey
                    };
                    var jsonWebKeySet = new JsonWebKeySet();
                    var publicKeysUsedToValidateSignature = ExtractPublicKeysForSignature(jwks);
                    var publicKeysUsedForClientEncryption = ExtractPrivateKeysForSignature(jwks);
                    var result = new JsonWebKeySet
                    {
                        Keys = new List<Dictionary<string, object>>()
                    };

                    result.Keys.AddRange(publicKeysUsedToValidateSignature);
                    result.Keys.AddRange(publicKeysUsedForClientEncryption);
                    string json = JsonConvert.SerializeObject(result);
                    var data = Encoding.UTF8.GetBytes(json);
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.Body.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                });
            });
            // 5. Use MVC.
            app.UseMvc();
        }

        private void ConfigureIdServer(IServiceCollection services)
        {
            services.AddSingleton(new SmsAuthenticationOptions());
            services.AddTransient<IEventPublisher, DefaultEventPublisher>();
            services.AddSingleton(_context.TwilioClient.Object);
            services.AddSingleton<ISubjectBuilder>(new DefaultSubjectBuilder());
            services.AddTransient<ISmsAuthenticationOperation, SmsAuthenticationOperation>();
            services.AddTransient<IGenerateAndSendSmsCodeOperation, GenerateAndSendSmsCodeOperation>();
            services.AddTransient<IAuthenticateResourceOwnerService, CustomAuthenticateResourceOwnerService>();
            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
            services.AddHostIdentityServer(_options)
                .AddSimpleIdentityServerCore(null, null, DefaultStores.Clients(_context), DefaultStores.Consents(), DefaultStores.JsonWebKeys(_context), null, DefaultStores.Users())
                .AddDefaultTokenStore()
                .AddSimpleIdentityServerJwt()
                .AddTechnicalLogging()
                .AddOpenidLogging()
                .AddOAuthLogging()
                .AddLogging()
                //.AddDefaultAccessTokenStore()
                .AddTransient<IAccountFilter, AccountFilter>()
                .AddSingleton<IFilterStore>(new DefaultFilterStore(null));
            services.AddSingleton(_context.ConfirmationCodeStore.Object);
            services.AddSingleton(sp => _context.Client);
            services.AddSingleton<IUsersClient>(sp =>
            {
                var baseUrl = _options.Scim.EndPoint;
                return new UsersClient(new Uri(baseUrl), sp.GetService<HttpClient>());
            });
            services.AddSingleton<IAccessTokenStore>(new TestAccessTokenStore());
        }

        private List<Dictionary<string, object>> ExtractPublicKeysForSignature(IEnumerable<JsonWebKey> jsonWebKeys)
        {
            var result = new List<Dictionary<string, object>>();
            var jsonWebKeysUsedForSignature = jsonWebKeys.Where(jwk => jwk.Use == Use.Sig && jwk.KeyOps.Contains(KeyOperations.Verify));
            foreach (var jsonWebKey in jsonWebKeysUsedForSignature)
            {
                var publicKeyInformation = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);
                var jsonWebKeyInformation = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
                publicKeyInformation.AddRange(jsonWebKeyInformation);
                result.Add(publicKeyInformation);
            }

            return result;
        }

        private List<Dictionary<string, object>> ExtractPrivateKeysForSignature(IEnumerable<JsonWebKey> jsonWebKeys)
        {
            var result = new List<Dictionary<string, object>>();
            // Retrieve all the JWK used by the client to encrypt the JWS
            var jsonWebKeysUsedForEncryption =
                jsonWebKeys.Where(jwk => jwk.Use == Use.Enc && jwk.KeyOps.Contains(KeyOperations.Encrypt));
            foreach (var jsonWebKey in jsonWebKeysUsedForEncryption)
            {
                var publicKeyInformation = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);
                var jsonWebKeyInformation = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
                publicKeyInformation.AddRange(jsonWebKeyInformation);
                result.Add(publicKeyInformation);
            }

            return result;
        }
    }
}
