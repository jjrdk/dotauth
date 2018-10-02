using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Helpers;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Startup
{
    public static class DefaultConfiguration
    {
        public static List<ResourceOwner> GetUsers()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Id = "administrator",
                    Claims = new List<Claim>
                    {
                        new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "administrator"),
                        new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role, "administrator")
                    },
                    Password = PasswordHelper.ComputeHash("password"),
                    IsLocalAccount = true
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
                    Code = Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                    Value = "the client {0} would like to access"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                    Value = "individual claims"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.NameCode,
                    Value = "Name"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginCode,
                    Value = "Login"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.PasswordCode,
                    Value = "Password"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.UserNameCode,
                    Value = "Username"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.ConfirmCode,
                    Value = "Confirm"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.CancelCode,
                    Value = "Cancel"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginLocalAccount,
                    Value = "Login with your local account"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginExternalAccount,
                    Value = "Login with your external account"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LinkToThePolicy,
                    Value = "policy"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Tos,
                    Value = "Terms of Service"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.SendCode,
                    Value = "Send code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Code,
                    Value = "Code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.EditResourceOwner,
                    Value = "Edit resource owner"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.YourName,
                    Value = "Your name"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.YourPassword,
                    Value = "Your password"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Email,
                    Value = "Email"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.YourEmail,
                    Value = "Your email"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                    Value = "Two authentication factor"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.UserIsUpdated,
                    Value = "User has been updated"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.SendConfirmationCode,
                    Value = "Send a confirmation code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Phone,
                    Value = "Phone"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.HashedPassword,
                    Value = "Hashed password"
                }
            };
        }
    }
}