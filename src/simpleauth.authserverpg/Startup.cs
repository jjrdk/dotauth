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

namespace SimpleAuth.AuthServerPg
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using SimpleAuth;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using Marten;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Stores.Marten;
    using SimpleAuth.UI;

    public class Startup
    {
        private const string SimpleAuthScheme = "simpleauth";
        private const string DefaultGoogleScopes = "openid,profile,email";
        private readonly IConfiguration _configuration;
        private readonly SimpleAuthOptions _options;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            bool.TryParse(_configuration["REDIRECT"], out var redirect);
            _options = new SimpleAuthOptions
            {
                RedirectToLogin = redirect,
                ApplicationName = _configuration["SERVER_NAME"] ?? "SimpleAuth",
                Users = sp => new MartenResourceOwnerStore(sp.GetRequiredService<IDocumentSession>),
                Clients =
                    sp => new MartenClientStore(sp.GetRequiredService<IDocumentSession>),
                Scopes = sp => new MartenScopeRepository(sp.GetRequiredService<IDocumentSession>),
                AccountFilters = sp => new MartenFilterStore(sp.GetRequiredService<IDocumentSession>),
                AuthorizationCodes =
                    sp => new MartenAuthorizationCodeStore(sp.GetRequiredService<IDocumentSession>),
                ConfirmationCodes = sp => new MartenConfirmationCodeStore(sp.GetRequiredService<IDocumentSession>),
                Consents = sp => new MartenConsentRepository(sp.GetRequiredService<IDocumentSession>),
                JsonWebKeys = sp => new MartenJwksRepository(sp.GetRequiredService<IDocumentSession>),
                Tickets = sp => new MartenTicketStore(sp.GetRequiredService<IDocumentSession>),
                Tokens = sp => new MartenTokenStore(sp.GetRequiredService<IDocumentSession>),
                ResourceSets = sp => new MartenResourceSetRepository(sp.GetRequiredService<IDocumentSession>),
                EventPublisher = sp => new LogEventPublisher(sp.GetRequiredService<ILogger<LogEventPublisher>>()),
                ClaimsIncludedInUserCreation = new[]
                {
                    ClaimTypes.Name,
                    ClaimTypes.Uri,
                    ClaimTypes.Country,
                    ClaimTypes.DateOfBirth,
                    ClaimTypes.Email,
                    ClaimTypes.Gender,
                    ClaimTypes.GivenName,
                    ClaimTypes.Locality,
                    ClaimTypes.PostalCode,
                    ClaimTypes.Role,
                    ClaimTypes.StateOrProvince,
                    ClaimTypes.StreetAddress,
                    ClaimTypes.Surname
                }
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IDocumentStore>(
                    provider =>
                    {
                        var options = new SimpleAuthMartenOptions(
                            _configuration["CONNECTIONSTRING"],
                            new MartenLoggerFacade(provider.GetService<ILogger<MartenLoggerFacade>>()));
                        return new DocumentStore(options);
                    })
                .AddTransient(sp => sp.GetService<IDocumentStore>().LightweightSession());

            services.AddResponseCompression(
                    x =>
                    {
                        x.EnableForHttps = true;
                        x.Providers.Add(
                            new GzipCompressionProvider(
                                new GzipCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                        x.Providers.Add(
                            new BrotliCompressionProvider(
                                new BrotliCompressionProviderOptions { Level = CompressionLevel.Optimal }));
                    })
                .AddLogging(log => { log.AddConsole(o => { o.IncludeScopes = true; }); })
                .AddAuthentication(
                    options =>
                    {
                        options.DefaultScheme = CookieNames.CookieName;
                        options.DefaultChallengeScheme = SimpleAuthScheme;
                    })
                .AddCookie(CookieNames.CookieName, opts => { opts.LoginPath = "/Authenticate"; })
                .AddOAuth(SimpleAuthScheme, '_' + SimpleAuthScheme, options => { })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    {
                        cfg.Authority = _configuration["OAUTH_AUTHORITY"];
                        cfg.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidIssuers = _configuration["OAUTH_VALID_ISSUERS"]
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .ToArray()
                        };
#if DEBUG
                        cfg.RequireHttpsMetadata = false;
#endif
                    });
            services.ConfigureOptions<ConfigureOAuthOptions>();

            if (!string.IsNullOrWhiteSpace(_configuration["Google:ClientId"]))
            {
                services.AddAuthentication(CookieNames.ExternalCookieName)
                    .AddCookie(CookieNames.ExternalCookieName)
                    .AddGoogle(
                        opts =>
                        {
                            opts.AccessType = "offline";
                            opts.ClientId = _configuration["Google:ClientId"];
                            opts.ClientSecret = _configuration["Google:ClientSecret"];
                            opts.SignInScheme = CookieNames.ExternalCookieName;
                            var scopes = _configuration["Google:Scopes"] ?? DefaultGoogleScopes;
                            foreach (var scope in scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim()))
                            {
                                opts.Scope.Add(scope);
                            }
                        });
            }

            services.AddSimpleAuth(
                _options,
                new[] { CookieNames.CookieName, JwtBearerDefaults.AuthenticationScheme, SimpleAuthScheme },
                assemblies: new[]
                {
                    (GetType().Namespace, GetType().Assembly),
                    (typeof(IDefaultUi).Namespace, typeof(IDefaultUi).Assembly)
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            //var lifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            //lifetime.ApplicationStarted.Register(
            //    () =>
            //    {
            //        using var session = app.ApplicationServices.GetService<IDocumentSession>();
            //        session.Store(DefaultConfiguration.GetClients());
            //        session.Store(DefaultConfiguration.GetJwks());
            //        session.Store(DefaultConfiguration.GetUsers());
            //        session.Store(DefaultConfiguration.GetScopes());
            //        session.Store(new GrantedToken { Id = Guid.NewGuid().ToString("N"), AccessToken = "abc" });
            //        session.SaveChanges();
            //    });
            app.UseResponseCompression().UseSimpleAuthMvc((typeof(IDefaultUi).Namespace, typeof(IDefaultUi).Assembly));
        }
    }


    public static class DefaultConfiguration
    {
        public static JsonWebKey[] GetJwks()
        {
            using var rsa = RSA.Create();
            return new[]
            {
                rsa.CreateJwk("1", JsonWebKeyUseNames.Sig, true, KeyOperations.Sign, KeyOperations.Verify),
                rsa.CreateJwk("2", JsonWebKeyUseNames.Enc, true, KeyOperations.Encrypt, KeyOperations.Decrypt),
                //rsa.CreateJwk("1", JsonWebKeyUseNames.Sig, false, KeyOperations.Sign, KeyOperations.Verify),
                //rsa.CreateJwk("2", JsonWebKeyUseNames.Enc, false, KeyOperations.Encrypt, KeyOperations.Decrypt)
            };
        }

        public static Client[] GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "api",
                    ClientName = "api",
                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "api"}},
                    JsonWebKeys =
                        new[]
                        {
                            "longsupersecretkey".CreateSignatureJwk(),
                            "longsupersecretkey".CreateEncryptionJwk()
                        }.ToJwks(),
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new[] {"register_client"},
                    GrantTypes =
                        new[] {GrantTypes.ClientCredentials, GrantTypes.Implicit, GrantTypes.AuthorizationCode},
                    ResponseTypes = new[] {ResponseTypeNames.Token, ResponseTypeNames.Code},
                    ApplicationType = ApplicationTypes.Native,
                    RedirectionUrls = new[] {new Uri("http://localhost:60000"),}
                },
                new Client
                {
                    ClientId = "mobileapp",
                    ClientName = "Admin client",
                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "Secret"}},
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    //LogoUri = null,
                    AllowedScopes = new[] {"openid", "profile", "role"},
                    GrantTypes =
                        new[]
                        {
                            GrantTypes.AuthorizationCode,
                            GrantTypes.ClientCredentials,
                            GrantTypes.Implicit,
                            GrantTypes.Password,
                            GrantTypes.RefreshToken,
                            GrantTypes.UmaTicket,
                            GrantTypes.ValidateBearer
                        },
                    JsonWebKeys =
                        new[] {"longsupersecretkey".CreateSignatureJwk(), "longsupersecretkey".CreateEncryptionJwk()}
                            .ToJwks(),
                    ResponseTypes = new[] {ResponseTypeNames.Token, ResponseTypeNames.IdToken},
                    IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256, // SecurityAlgorithms.RsaSha256,
                    ApplicationType = ApplicationTypes.Native
                },
                new Client
                {
                    ClientId = "web",
                    ClientName = "web",
                    AllowedScopes = new[] {"openid", "role", "profile", "email", "manager", "uma_protection"},
                    ApplicationType = ApplicationTypes.Web,
                    GrantTypes = GrantTypes.All,
                    RequirePkce = true,
                    RedirectionUrls =
                        new[]
                        {
                            new Uri("http://localhost:4200/login"),
                            new Uri("https://localhost:50001/signin-oidc"),
                            new Uri("https://localhost:5001/signin-oidc"),
                            new Uri("https://localhost:5001/callback"),
                        },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.None,
                    PostLogoutRedirectUris = new[] {new Uri("http://localhost:4200/login")},
                    ResponseTypes = ResponseTypeNames.All,
                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    UserClaimsToIncludeInAuthToken = new[]
                    {
                        new Regex($"^{OpenIdClaimTypes.Subject}$", RegexOptions.Compiled),
                        new Regex($"^{OpenIdClaimTypes.Role}$", RegexOptions.Compiled)
                    },
                }
            };
        }

        public static Scope[] GetScopes()
        {
            return new[]
        {
            new Scope
            {
                Name = "openid",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = Strings.AccessToOpenIdScope,
                Type = ScopeTypes.ResourceOwner,
                Claims = Array.Empty<string>()
            },
            new Scope
            {
                Name = "profile",
                IsExposed = true,
                Description = Strings.AccessToProfileInformation,
                Claims = new[]
                {
                    OpenIdClaimTypes.Name,
                    OpenIdClaimTypes.FamilyName,
                    OpenIdClaimTypes.GivenName,
                    OpenIdClaimTypes.MiddleName,
                    OpenIdClaimTypes.NickName,
                    OpenIdClaimTypes.PreferredUserName,
                    OpenIdClaimTypes.Profile,
                    OpenIdClaimTypes.Picture,
                    OpenIdClaimTypes.WebSite,
                    OpenIdClaimTypes.Gender,
                    OpenIdClaimTypes.BirthDate,
                    OpenIdClaimTypes.ZoneInfo,
                    OpenIdClaimTypes.Locale,
                    OpenIdClaimTypes.UpdatedAt
                },
                Type = ScopeTypes.ResourceOwner,
                IsDisplayedInConsent = true
            },
            new Scope
            {
                Name = "email",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = Strings.AccessToEmailAddresses,
                Claims = new[] {OpenIdClaimTypes.Email, OpenIdClaimTypes.EmailVerified},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "address",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = Strings.AccessToAddressInformation,
                Claims = new[] {OpenIdClaimTypes.Address},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "phone",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = Strings.AccessToPhoneInformation,
                Claims = new[] {OpenIdClaimTypes.PhoneNumber, OpenIdClaimTypes.PhoneNumberVerified},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Name = "role",
                IsExposed = true,
                IsDisplayedInConsent = true,
                Description = Strings.AccessToRoles,
                Claims = new[] {OpenIdClaimTypes.Role},
                Type = ScopeTypes.ResourceOwner
            },
            new Scope
            {
                Claims = new[] {OpenIdClaimTypes.Role},
                Name = "register_client",
                IsExposed = false,
                IsDisplayedInConsent = false,
                Description = Strings.RegisterAClient,
                Type = ScopeTypes.ProtectedApi
            },
            new Scope
            {
                Claims = new[] {OpenIdClaimTypes.Role},
                Description = Strings.ManageServerResources,
                IsDisplayedInConsent = false,
                IsExposed = false,
                Name = "manager",
                Type = ScopeTypes.ProtectedApi
            },
            new Scope
            {
                Claims = new[] {OpenIdClaimTypes.Subject},
                Description = Strings.ManageUma,
                IsDisplayedInConsent = true,
                IsExposed = true,
                Name = "uma_protection",
                Type = ScopeTypes.ProtectedApi
            }
        };
        }

        public static ResourceOwner[] GetUsers()
        {
            return new[]
            {
                new ResourceOwner
                {
                    Subject = "administrator",
                    Claims = new[]
                    {
                        new Claim(StandardClaimNames.Subject, "administrator"),
                        new Claim("role", "administrator"),
                        new Claim("role", "uma_admin"),
                        new Claim(OpenIdClaimTypes.Name, "Anne Admin"),
                        new Claim(OpenIdClaimTypes.Email, "admin@server.com"),
                        new Claim(OpenIdClaimTypes.EmailVerified, bool.TrueString)
                    },
                    Password = "password".ToSha256Hash(),
                    IsLocalAccount = true,
                    CreateDateTime = DateTimeOffset.UtcNow,
                },
                new ResourceOwner
                {
                    Subject = "user",
                    Claims = new[]
                    {
                        new Claim(StandardClaimNames.Subject, "user"),
                        new Claim(OpenIdClaimTypes.Name, "Anne User"),
                        new Claim(OpenIdClaimTypes.Email, "user@server.com"),
                        new Claim(OpenIdClaimTypes.EmailVerified, bool.TrueString)
                    },
                    Password = "password".ToSha256Hash(),
                    IsLocalAccount = true,
                    CreateDateTime = DateTimeOffset.UtcNow,
                },
            };
        }
    }
}
