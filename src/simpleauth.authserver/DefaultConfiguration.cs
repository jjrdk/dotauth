namespace SimpleAuth.AuthServer
{
    using Helpers;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.DTOs;
    using Shared.Models;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;

    public static class DefaultConfiguration
    {
        public static List<Client> GetClients()
        {
            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "mycert.pfx");
            var certificate = new X509Certificate2(path, "simpleauth", X509KeyStorageFlags.Exportable);
            return new List<Client>
            {
                new Client
                {
                    ClientId = "api",
                    ClientName = "api",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "api"
                        }
                    },
                    JsonWebKeys = new[]{certificate
                        .CreateJwk(JsonWebKeyUseNames.Sig,
                            KeyOperations.Sign,
                            KeyOperations.Verify),
                        certificate.CreateJwk(JsonWebKeyUseNames.Enc,
                            KeyOperations.Encrypt,
                            KeyOperations.Decrypt)
                    }.ToJwks(),
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    PolicyUri = new Uri("http://openid.net"),
                    TosUri = new Uri("http://openid.net"),
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "register_client"
                        }
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.client_credentials
                    },
                    ResponseTypes = new List<string>
                    {
                        ResponseTypeNames.Token
                    },
                    ApplicationType = ApplicationTypes.native
                }
            };
        }

        public static List<ResourceOwner> GetUsers()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Id = "administrator",
                    Claims = new List<Claim>
                    {
                        new Claim(StandardClaimNames.Subject, "administrator"),
                        new Claim("role", "administrator")
                    },
                    Password = "password".ToSha256Hash(),
                    IsLocalAccount = true,
                    CreateDateTime = DateTime.UtcNow,
                    UserProfile = new ScimUser()
                }
            };
        }

        public static List<Translation> GetTranslations()
        {
            return new List<Translation>
            {
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                    Value = "the client {0} would like to access"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.IndividualClaimsCode,
                    Value = "individual claims"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.NameCode,
                    Value = "Name"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.LoginCode,
                    Value = "Login"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.PasswordCode,
                    Value = "Password"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.UserNameCode,
                    Value = "Username"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.ConfirmCode,
                    Value = "Confirm"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.CancelCode,
                    Value = "Cancel"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.LoginLocalAccount,
                    Value = "Login with your local account"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.LoginExternalAccount,
                    Value = "Login with your external account"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.LinkToThePolicy,
                    Value = "policy"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.Tos,
                    Value = "Terms of Service"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.SendCode,
                    Value = "Send code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.Code,
                    Value = "Code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.EditResourceOwner,
                    Value = "Edit resource owner"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.YourName,
                    Value = "Your name"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.YourPassword,
                    Value = "Your password"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.Email,
                    Value = "Email"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.YourEmail,
                    Value = "Your email"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.TwoAuthenticationFactor,
                    Value = "Two authentication factor"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.UserIsUpdated,
                    Value = "User has been updated"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.SendConfirmationCode,
                    Value = "Send a confirmation code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.Phone,
                    Value = "Phone"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = CoreConstants.StandardTranslationCodes.HashedPassword,
                    Value = "Hashed password"
                },
                //TODO: Add translations
                new Translation
                {
                    LanguageTag = "en",
                    Code = "[remember_my_login]",
                    Value = "Remember me"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = "remember_my_login",
                    Value = "Remember me"
                },
            };
        }
    }
}