namespace DotAuth.Tests.WebSite.Controller;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

public sealed class PasswordUiEndpointHandlersTests
{
    [Fact]
    public async Task When_Posting_External_Login_Form_Then_Provider_Is_Read_From_Form_And_Challenge_Is_Issued()
    {
        var authenticationService = Substitute.For<IAuthenticationService>();
        var httpContext = CreateHttpContext(
            "/authenticate/externallogin",
            new Dictionary<string, string> { ["provider"] = "Google" });

        var result = await PasswordUiEndpointHandlers.ExternalLogin(httpContext, authenticationService);

        await authenticationService.Received(1).ChallengeAsync(
            httpContext,
            "Google",
            Arg.Is<AuthenticationProperties>(props => props.RedirectUri == "http://localhost:8001/authenticate/logincallback"));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task When_Posting_External_OpenId_Login_Form_Then_Provider_Is_Read_From_Form_And_Challenge_Is_Issued()
    {
        var authenticationService = Substitute.For<IAuthenticationService>();
        var httpContext = CreateHttpContext(
            "/pwd/authenticate/externalloginopenid",
            new Dictionary<string, string> { ["provider"] = "Google" },
            new Dictionary<string, string> { ["code"] = "protected-openid-request" });

        var result = await PasswordUiEndpointHandlers.ExternalLoginOpenId(
            httpContext,
            authenticationService,
            new RuntimeSettings(allowHttp: true),
            NullLoggerFactory.Instance);

        await authenticationService.Received(1).ChallengeAsync(
            httpContext,
            "Google",
            Arg.Is<AuthenticationProperties>(props =>
                props.RedirectUri != null
                && props.RedirectUri.StartsWith("http://localhost:8001/authenticate/logincallbackopenid?code=")));
        var setCookieHeaders = httpContext.Response.Headers.SetCookie.ToArray();
        Assert.Contains(setCookieHeaders, header => header is not null && header.Contains("ExternalAuth-"));
        Assert.NotNull(result);
    }

    private static DefaultHttpContext CreateHttpContext(
        string path,
        IReadOnlyDictionary<string, string> formValues,
        IReadOnlyDictionary<string, string>? queryValues = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost", 8001);
        httpContext.Request.Path = path;
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        httpContext.Features.Set<IFormFeature>(
            new FormFeature(new FormCollection(formValues.ToDictionary(kvp => kvp.Key, kvp => new Microsoft.Extensions.Primitives.StringValues(kvp.Value)))));

        if (queryValues != null && queryValues.Count > 0)
        {
            httpContext.Request.QueryString = QueryString.Create(queryValues.Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value)));
        }

        return httpContext;
    }
}



