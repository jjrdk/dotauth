using SimpleIdentityServer.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Repositories
{
    using Shared.Models;
    using Shared.Repositories;

    internal sealed class DefaultTranslationRepository : ITranslationRepository
    {
        public ICollection<Translation> _translations;

        private readonly List<Translation> DEFAULT_TRANSLATIONS = new List<Translation>
        {
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                Value = "the client {0} would like to access"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.IndividualClaimsCode,
                Value = "individual claims"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.NameCode,
                Value = "Name"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.LoginCode,
                Value = "Login"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.PasswordCode,
                Value = "Password"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.UserNameCode,
                Value = "Username"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.ConfirmCode,
                Value = "Confirm"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.CancelCode,
                Value = "Cancel"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.LoginLocalAccount,
                Value = "Login with your local account"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.LoginExternalAccount,
                Value = "Login with your external account"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.LinkToThePolicy,
                Value = "policy"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.Tos,
                Value = "Terms of Service"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.SendCode,
                Value = "Send code"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.Code,
                Value = "Code"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.EditResourceOwner,
                Value = "Edit resource owner"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.YourName,
                Value = "Your name"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.YourPassword,
                Value = "Your password"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.Email,
                Value = "Email"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.YourEmail,
                Value = "Your email"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                Value = "Two authentication factor"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.UserIsUpdated,
                Value = "User has been updated"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.SendConfirmationCode,
                Value = "Send a confirmation code"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.Phone,
                Value = "Phone"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.HashedPassword,
                Value = "Hashed password"
            }
        };

        public DefaultTranslationRepository(IReadOnlyCollection<Translation> translations)
        {
            _translations = translations == null || translations.Count == 0
                ? DEFAULT_TRANSLATIONS
                : translations.ToList();
        }

        public Task<Translation> GetAsync(string languageTag, string code)
        {
            var translation = _translations.FirstOrDefault(t => t.LanguageTag == languageTag && t.Code == code);
            if (translation == null)
            {
                return Task.FromResult((Translation)null);
            }

            return Task.FromResult(translation.Copy());
        }

        public Task<ICollection<Translation>> GetAsync(string languageTag)
        {
            ICollection<Translation> result = _translations.Where(t => t.LanguageTag == languageTag).Select(t => t.Copy()).ToList();
            return Task.FromResult(result);
        }

        public Task<ICollection<string>> GetLanguageTagsAsync()
        {
            ICollection<string> result = _translations.Select(t => t.LanguageTag).ToList();
            return Task.FromResult(result);
        }
    }
}
