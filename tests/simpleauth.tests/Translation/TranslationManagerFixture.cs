namespace SimpleAuth.Tests.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Translation;
    using Xunit;

    public sealed class TranslationManagerFixture
    {
        private OAuthConfigurationOptions _oauthConfigurationOptions;
        private Mock<ITranslationRepository> _translationRepositoryFake;
        private ITranslationManager _translationManager;

        [Fact]
        public async Task When_Passing_No_Translation_Codes_Then_Exception_Is_Raised()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _translationManager.GetTranslationsAsync(string.Empty, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Preferred_Language_Then_Codes_Are_Translated_And_Default_Language_Is_Used()
        {            InitializeFakeObjects();
            var translationCodes = new List<string>
            {
                "translation_code"
            };
            var translation = new Translation
            {
                Code = "code",
                Value = "value"
            };
            _translationRepositoryFake.Setup(t => t.GetAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult(translation)); ;

                        var result = await _translationManager.GetTranslationsAsync(string.Empty, translationCodes).ConfigureAwait(false);

                        Assert.True(result.Count == 1);
            Assert.True(result.First().Key == translation.Code && result.First().Value == translation.Value);
        }

        [Fact]
        public async Task When_Passing_No_Preferred_Language_And_There_Is_No_Translation_Then_Codes_Are_Returned()
        {            InitializeFakeObjects();
            var translationCodes = new List<string>
            {
                "translation_code"
            };

            _translationRepositoryFake.Setup(t => t.GetAsync(It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(Task.FromResult((Translation)null)); 

                        var result = await _translationManager.GetTranslationsAsync(string.Empty, translationCodes).ConfigureAwait(false);

                        Assert.True(result.Count == 1);
            Assert.True(result.First().Key == "translation_code" 
                && result.First().Value == "[translation_code]");
        }

        private void InitializeFakeObjects()
        {
            _oauthConfigurationOptions = new OAuthConfigurationOptions();
            _translationRepositoryFake = new Mock<ITranslationRepository>();
            _translationManager = new TranslationManager(
                _oauthConfigurationOptions,
                _translationRepositoryFake.Object);
        }
    }
}
