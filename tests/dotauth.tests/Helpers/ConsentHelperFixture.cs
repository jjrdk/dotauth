namespace DotAuth.Tests.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using NSubstitute;
using Xunit;

public sealed class ConsentHelperFixture
{
    private readonly IConsentRepository _consentRepositoryFake;

    public ConsentHelperFixture()
    {
        _consentRepositoryFake = Substitute.For<IConsentRepository>();
    }

    [Fact]
    public async Task When_No_Consent_Has_Been_Given_By_The_Resource_Owner_Then_Null_Is_Returned()
    {
        const string subject = "subject";
        var authorizationParameter = new AuthorizationParameter();

        _consentRepositoryFake
            .GetConsentsForGivenUser(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((IReadOnlyCollection<Consent>)null));

        var result = await _consentRepositoryFake
            .GetConfirmedConsents(subject, authorizationParameter, CancellationToken.None)
            ;

        Assert.Null(result);
    }

    [Fact]
    public async Task When_A_Consent_Has_Been_Given_For_Claim_Name_Then_Consent_Is_Returned()
    {
        const string subject = "subject";
        const string claimName = "name";
        const string clientId = "clientId";
        var authorizationParameter = new AuthorizationParameter
        {
            Claims = new ClaimsParameter
            {
                UserInfo = new[] { new ClaimParameter { Name = claimName } }
            },
            ClientId = clientId
        };
        IReadOnlyCollection<Consent> consents = new List<Consent>
        {
            new() { Claims = new[] { claimName }, ClientId = clientId }
        };

        _consentRepositoryFake.GetConsentsForGivenUser(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(consents);

        var result = await _consentRepositoryFake
            .GetConfirmedConsents(subject, authorizationParameter, CancellationToken.None)
            ;

        Assert.NotNull(result);
        Assert.Single(result.Claims);
        Assert.Equal(claimName, result.Claims.First());
    }

    [Fact]
    public async Task When_A_Consent_Has_Been_Given_For_Scope_Profile_Then_Consent_Is_Returned()
    {
        const string subject = "subject";
        const string scope = "profile";
        const string clientId = "clientId";
        var authorizationParameter = new AuthorizationParameter { ClientId = clientId, Scope = scope };
        IReadOnlyCollection<Consent> consents = new List<Consent>
        {
            new() { ClientId = clientId, GrantedScopes = new[] { scope } }
        };

        _consentRepositoryFake.GetConsentsForGivenUser(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(consents);

        var result = await _consentRepositoryFake
            .GetConfirmedConsents(subject, authorizationParameter, CancellationToken.None)
            ;

        Assert.NotNull(result);
        Assert.Single(result.GrantedScopes);
        Assert.Equal(scope, result.GrantedScopes.First());
    }

    [Fact]
    public async Task
        When_Consent_Has_Been_Assigned_To_OpenId_Profile_And_Request_Consent_For_Scope_OpenId_Profile_Email_Then_Null_Is_Returned()
    {
        const string subject = "subject";
        const string openIdScope = "openid";
        const string profileScope = "profile";
        const string emailScope = "email";
        const string clientId = "clientId";
        var authorizationParameter = new AuthorizationParameter
        {
            ClientId = clientId,
            Scope = openIdScope + " " + profileScope + " " + emailScope
        };
        IReadOnlyCollection<Consent> consents = new List<Consent>
        {
            new()
            {
                ClientId = clientId, GrantedScopes = new[] { profileScope, openIdScope }
            }
        };

        _consentRepositoryFake.GetConsentsForGivenUser(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(consents);

        var result = await _consentRepositoryFake
            .GetConfirmedConsents(subject, authorizationParameter, CancellationToken.None)
            ;

        Assert.Null(result);
    }
}
