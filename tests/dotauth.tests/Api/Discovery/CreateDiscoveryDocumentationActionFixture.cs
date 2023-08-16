namespace DotAuth.Tests.Api.Discovery;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Discovery;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using NSubstitute;
using Xunit;

public sealed class CreateDiscoveryDocumentationActionFixture
{
    private readonly IScopeRepository _scopeRepositoryStub;
    private readonly DiscoveryActions _createDiscoveryDocumentationAction;

    public CreateDiscoveryDocumentationActionFixture()
    {
        _scopeRepositoryStub = Substitute.For<IScopeRepository>();
        _createDiscoveryDocumentationAction = new DiscoveryActions(_scopeRepositoryStub);
    }

    [Fact]
    public async Task When_Expose_Two_Scopes_Then_DiscoveryDocument_Is_Correct()
    {
        const string firstScopeName = "firstScopeName";
        const string secondScopeName = "secondScopeName";
        const string notExposedScopeName = "notExposedScopeName";
        var scopes = new[]
        {
            new Scope {IsExposed = true, Name = firstScopeName},
            new Scope {IsExposed = true, Name = secondScopeName},
            new Scope {IsExposed = false, Name = secondScopeName}
        };

        _scopeRepositoryStub.GetAll(Arg.Any<CancellationToken>()).Returns(scopes);

        var discoveryInformation = await _createDiscoveryDocumentationAction
            .CreateDiscoveryInformation("http://test", CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(2, discoveryInformation.ScopesSupported.Length);
        Assert.Contains(firstScopeName, discoveryInformation.ScopesSupported);
        Assert.Contains(secondScopeName, discoveryInformation.ScopesSupported);
        Assert.DoesNotContain(notExposedScopeName, discoveryInformation.ScopesSupported);
    }
}
