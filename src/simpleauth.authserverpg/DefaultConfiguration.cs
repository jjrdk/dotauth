//namespace SimpleAuth.AuthServerPg
//{
//    using System;
//    using System.Linq;
//    using System.Security.Claims;
//    using System.Security.Cryptography;
//    using System.Text.RegularExpressions;
//    using Marten;
//    using Microsoft.AspNetCore.Builder;
//    using Microsoft.Extensions.Hosting;
//    using Microsoft.IdentityModel.Tokens;
//    using SimpleAuth.Properties;
//    using SimpleAuth.Shared;
//    using SimpleAuth.Shared.Models;
//    using SimpleAuth.Stores.Marten;

//    public static class DefaultConfiguration
//    {
//        public static void Seed(this IApplicationBuilder app)
//        {
//            var lifetime = (IHostApplicationLifetime)app.ApplicationServices.GetService(typeof(IHostApplicationLifetime));
//            lifetime!.ApplicationStarted.Register(
//                () =>
//                {
//                    using var session = (IDocumentSession)app.ApplicationServices.GetService(typeof(IDocumentSession));
//                    session!.Store(DefaultConfiguration.GetClients());
//                    session.Store(DefaultConfiguration.GetJwks());
//                    session.Store(DefaultConfiguration.GetUsers());
//                    session.Store(DefaultConfiguration.GetScopes());
//                    session.Store(new GrantedToken { Id = Guid.NewGuid().ToString("N"), AccessToken = "abc" });
//                    session.SaveChanges();
//                });
//        }

//        public static JsonWebKeyContainer[] GetJwks()
//        {
//            using var rsa = RSA.Create();
//            return new[]
//                {
//                    rsa.CreateJwk("1", JsonWebKeyUseNames.Sig, true, KeyOperations.Sign, KeyOperations.Verify),
//                    rsa.CreateJwk("2", JsonWebKeyUseNames.Enc, true, KeyOperations.Encrypt, KeyOperations.Decrypt),
//                    rsa.CreateJwk("1", JsonWebKeyUseNames.Sig, false, KeyOperations.Sign, KeyOperations.Verify),
//                    rsa.CreateJwk("2", JsonWebKeyUseNames.Enc, false, KeyOperations.Encrypt, KeyOperations.Decrypt)
//                }
//                .Select(key => new JsonWebKeyContainer { Id = Id.Create(), Jwk = key })
//                .ToArray();
//        }

//        public static Client[] GetClients()
//        {
//            return new[]
//            {
//                new Client
//                {
//                    ClientId = "web",
//                    ClientName = "web",
//                    AllowedScopes = new[] {"openid", "role", "profile", "email", "manager", "uma_protection"},
//                    ApplicationType = ApplicationTypes.Web,
//                    GrantTypes = GrantTypes.All,
//                    RequirePkce = true,
//                    RedirectionUrls =
//                        new[]
//                        {
//                            new Uri("https://odin/signin-oidc"),
//                            new Uri("https://odin/callback"),
//                            new Uri("https://sira/signin-oidc"),
//                            new Uri("https://sira/callback"),
//                        },
//                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.None,
//                    PostLogoutRedirectUris = new[] {new Uri("https://odin")},
//                    ResponseTypes = ResponseTypeNames.All,
//                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
//                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
//                    UserClaimsToIncludeInAuthToken = new[]
//                    {
//                        new Regex($"^{OpenIdClaimTypes.Subject}$", RegexOptions.Compiled),
//                        new Regex($"^{OpenIdClaimTypes.Role}$", RegexOptions.Compiled)
//                    },
//                },
//                new Client
//                {
//                    ClientId = "health",
//                    ClientName = "health",
//                    AllowedScopes = new[] {"openid", "role", "profile", "email", "manager", "uma_protection"},
//                    ApplicationType = ApplicationTypes.Web,
//                    GrantTypes = GrantTypes.All,
//                    RequirePkce = true,
//                    RedirectionUrls =
//                        new[]
//                        {
//                            new Uri("https://odin/signin-oidc"),
//                            new Uri("https://odin/callback"),
//                            new Uri("https://sira/signin-oidc"),
//                            new Uri("https://sira/callback"),
//                        },
//                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.None,
//                    PostLogoutRedirectUris = new[] {new Uri("https://odin")},
//                    ResponseTypes = ResponseTypeNames.All,
//                    Secrets = new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
//                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
//                    UserClaimsToIncludeInAuthToken = new[]
//                    {
//                        new Regex($"^{OpenIdClaimTypes.Subject}$", RegexOptions.Compiled),
//                        new Regex($"^{OpenIdClaimTypes.Role}$", RegexOptions.Compiled)
//                    },
//                },
//            };
//        }

//        public static Scope[] GetScopes()
//        {
//            return new[]
//            {
//                new Scope
//                {
//                    Name = "openid",
//                    IsExposed = true,
//                    IsDisplayedInConsent = true,
//                    Description = Strings.AccessToOpenIdScope,
//                    Type = ScopeTypes.ResourceOwner,
//                    Claims = Array.Empty<string>()
//                },
//                new Scope
//                {
//                    Name = "profile",
//                    IsExposed = true,
//                    Description = Strings.AccessToProfileInformation,
//                    Claims = new[]
//                    {
//                        OpenIdClaimTypes.Name,
//                        OpenIdClaimTypes.FamilyName,
//                        OpenIdClaimTypes.GivenName,
//                        OpenIdClaimTypes.MiddleName,
//                        OpenIdClaimTypes.NickName,
//                        OpenIdClaimTypes.PreferredUserName,
//                        OpenIdClaimTypes.Profile,
//                        OpenIdClaimTypes.Picture,
//                        OpenIdClaimTypes.WebSite,
//                        OpenIdClaimTypes.Gender,
//                        OpenIdClaimTypes.BirthDate,
//                        OpenIdClaimTypes.ZoneInfo,
//                        OpenIdClaimTypes.Locale,
//                        OpenIdClaimTypes.UpdatedAt
//                    },
//                    Type = ScopeTypes.ResourceOwner,
//                    IsDisplayedInConsent = true
//                },
//                new Scope
//                {
//                    Name = "email",
//                    IsExposed = true,
//                    IsDisplayedInConsent = true,
//                    Description = Strings.AccessToEmailAddresses,
//                    Claims = new[] {OpenIdClaimTypes.Email, OpenIdClaimTypes.EmailVerified},
//                    Type = ScopeTypes.ResourceOwner
//                },
//                new Scope
//                {
//                    Name = "address",
//                    IsExposed = true,
//                    IsDisplayedInConsent = true,
//                    Description = Strings.AccessToAddressInformation,
//                    Claims = new[] {OpenIdClaimTypes.Address},
//                    Type = ScopeTypes.ResourceOwner
//                },
//                new Scope
//                {
//                    Name = "phone",
//                    IsExposed = true,
//                    IsDisplayedInConsent = true,
//                    Description = Strings.AccessToPhoneInformation,
//                    Claims = new[] {OpenIdClaimTypes.PhoneNumber, OpenIdClaimTypes.PhoneNumberVerified},
//                    Type = ScopeTypes.ResourceOwner
//                },
//                new Scope
//                {
//                    Name = "role",
//                    IsExposed = true,
//                    IsDisplayedInConsent = true,
//                    Description = Strings.AccessToRoles,
//                    Claims = new[] {OpenIdClaimTypes.Role},
//                    Type = ScopeTypes.ResourceOwner
//                },
//                new Scope
//                {
//                    Claims = new[] {OpenIdClaimTypes.Role},
//                    Name = "register_client",
//                    IsExposed = false,
//                    IsDisplayedInConsent = false,
//                    Description = Strings.RegisterAClient,
//                    Type = ScopeTypes.ProtectedApi
//                },
//                new Scope
//                {
//                    Claims = new[] {OpenIdClaimTypes.Role},
//                    Description = Strings.ManageServerResources,
//                    IsDisplayedInConsent = false,
//                    IsExposed = false,
//                    Name = "manager",
//                    Type = ScopeTypes.ProtectedApi
//                },
//                new Scope
//                {
//                    Claims = new[] {OpenIdClaimTypes.Subject},
//                    Description = Strings.ManageUma,
//                    IsDisplayedInConsent = true,
//                    IsExposed = true,
//                    Name = "uma_protection",
//                    Type = ScopeTypes.ProtectedApi
//                }
//            };
//        }

//        public static ResourceOwner[] GetUsers()
//        {
//            return new[]
//            {
//                new ResourceOwner
//                {
//                    Subject = "administrator",
//                    Claims = new[]
//                    {
//                        new Claim(StandardClaimNames.Subject, "administrator"),
//                        new Claim("role", "administrator"),
//                        new Claim("role", "uma_admin"),
//                        new Claim(OpenIdClaimTypes.Name, "Anne Admin"),
//                        new Claim(OpenIdClaimTypes.Email, "admin@server.com"),
//                        new Claim(OpenIdClaimTypes.EmailVerified, bool.TrueString)
//                    },
//                    Password = "password".ToSha256Hash(),
//                    IsLocalAccount = true,
//                    CreateDateTime = DateTimeOffset.UtcNow,
//                },
//                new ResourceOwner
//                {
//                    Subject = "user",
//                    Claims = new[]
//                    {
//                        new Claim(StandardClaimNames.Subject, "user"),
//                        new Claim(OpenIdClaimTypes.Name, "Anne User"),
//                        new Claim(OpenIdClaimTypes.Email, "user@server.com"),
//                        new Claim(OpenIdClaimTypes.EmailVerified, bool.TrueString)
//                    },
//                    Password = "password".ToSha256Hash(),
//                    IsLocalAccount = true,
//                    CreateDateTime = DateTimeOffset.UtcNow,
//                },
//            };
//        }
//    }
//}