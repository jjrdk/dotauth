namespace SimpleIdentityServer.Core.Translation
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITranslationManager
    {
        Task<Dictionary<string, string>> GetTranslationsAsync(string concatenateListOfCodeLanguages, List<string> translationCodes);
    }
}