namespace DotAuth.Tests.Policies;

using System.Security.Claims;
using System.Threading.Tasks;
using DotAuth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class UiAuthorizationPolicyFixture
{
    [Fact]
    public async Task When_Local_Cookie_User_Has_Administrator_Role_Then_Manager_Policy_Is_Satisfied_Without_Scope_Claim()
    {
        await using var provider = CreateServiceProvider();
        var authorizationService = provider.GetRequiredService<IAuthorizationService>();
        var principal = CreatePrincipal(
            CookieNames.CookieName,
            new Claim(OpenIdClaimTypes.Subject, "administrator"),
            new Claim(OpenIdClaimTypes.Role, "administrator"));

        var result = await authorizationService.AuthorizeAsync(principal, resource: null, "manager");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task When_Local_Cookie_User_Is_Authenticated_Then_UmaProtection_Policy_Is_Satisfied_Without_Scope_Claim()
    {
        await using var provider = CreateServiceProvider();
        var authorizationService = provider.GetRequiredService<IAuthorizationService>();
        var principal = CreatePrincipal(
            CookieNames.CookieName,
            new Claim(OpenIdClaimTypes.Subject, "user"));

        var result = await authorizationService.AuthorizeAsync(principal, resource: null, "UmaProtection");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task When_Local_Cookie_User_Lacks_Administrator_Role_Then_Manager_Policy_Is_Not_Satisfied()
    {
        await using var provider = CreateServiceProvider();
        var authorizationService = provider.GetRequiredService<IAuthorizationService>();
        var principal = CreatePrincipal(
            CookieNames.CookieName,
            new Claim(OpenIdClaimTypes.Subject, "user"));

        var result = await authorizationService.AuthorizeAsync(principal, resource: null, "manager");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task When_Bearer_User_Lacks_UmaProtection_Scope_Then_UmaProtection_Policy_Is_Not_Satisfied()
    {
        await using var provider = CreateServiceProvider();
        var authorizationService = provider.GetRequiredService<IAuthorizationService>();
        var principal = CreatePrincipal(
            "Bearer",
            new Claim(OpenIdClaimTypes.Subject, "user"));

        var result = await authorizationService.AuthorizeAsync(principal, resource: null, "UmaProtection");

        Assert.False(result.Succeeded);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options =>
        {
            options.AddAuthPolicies((OpenIdClaimTypes.Role, "administrator"), [CookieNames.CookieName, "Bearer"]);
        });
        return services.BuildServiceProvider();
    }

    private static ClaimsPrincipal CreatePrincipal(string authenticationType, params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));
    }
}


