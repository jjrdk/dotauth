using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Repositories
{
    internal sealed class DefaultTranslationRepository : ITranslationRepository
    {
        public ICollection<Common.Models.Translation> _translations;

        private readonly List<Common.Models.Translation> DEFAULT_TRANSLATIONS = new List<Common.Models.Translation>
        {
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                Value = "the client {0} would like to access"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.IndividualClaimsCode,
                Value = "individual claims"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.NameCode,
                Value = "Name"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.LoginCode,
                Value = "Login"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.PasswordCode,
                Value = "Password"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.UserNameCode,
                Value = "Username"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.ConfirmCode,
                Value = "Confirm"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.CancelCode,
                Value = "Cancel"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.LoginLocalAccount,
                Value = "Login with your local account"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.LoginExternalAccount,
                Value = "Login with your external account"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.LinkToThePolicy,
                Value = "policy"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.Tos,
                Value = "Terms of Service"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.SendCode,
                Value = "Send code"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.Code,
                Value = "Code"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.EditResourceOwner,
                Value = "Edit resource owner"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.YourName,
                Value = "Your name"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.YourPassword,
                Value = "Your password"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.Email,
                Value = "Email"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.YourEmail,
                Value = "Your email"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                Value = "Two authentication factor"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.UserIsUpdated,
                Value = "User has been updated"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.SendConfirmationCode,
                Value = "Send a confirmation code"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.Phone,
                Value = "Phone"
            },
            new Common.Models.Translation
            {
                LanguageTag = "en",
                Code = Constants.StandardTranslationCodes.HashedPassword,
                Value = "Hashed password"
            }
        };

        public DefaultTranslationRepository(IReadOnlyCollection<Common.Models.Translation> translations)
        {
            _translations = translations == null || translations.Count == 0
                ? DEFAULT_TRANSLATIONS
                : translations.ToList();
        }

        public Task<Common.Models.Translation> GetAsync(string languageTag, string code)
        {
            var translation = _translations.FirstOrDefault(t => t.LanguageTag == languageTag && t.Code == code);
            if (translation == null)
            {
                return Task.FromResult((Common.Models.Translation)null);
            }

            return Task.FromResult(translation.Copy());
        }

        public Task<ICollection<Common.Models.Translation>> GetAsync(string languageTag)
        {
            ICollection<Common.Models.Translation> result = _translations.Where(t => t.LanguageTag == languageTag).Select(t => t.Copy()).ToList();
            return Task.FromResult(result);
        }

        public Task<ICollection<string>> GetLanguageTagsAsync()
        {
            ICollection<string> result = _translations.Select(t => t.LanguageTag).ToList();
            return Task.FromResult(result);
        }
    }
}
