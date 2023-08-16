namespace DotAuth.Tests.Helpers;

using System.Collections.Generic;
using DotAuth.Api.Authorization;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using Xunit;

public sealed class AuthorizationFlowHelperFixture
{
    [Fact]
    public void WhenPassingEmptyListOfResponseTypesThenErrorIsReturned()
    {
        const string state = "state";

        var exception = Assert.IsType<Option<AuthorizationFlow>.Error>(new List<string>().GetAuthorizationFlow(state));

        var expected = new Option<AuthorizationFlow>.Error(
            new ErrorDetails
            {
                Title = ErrorCodes.InvalidRequest,
                Detail = Strings.TheAuthorizationFlowIsNotSupported
            },
            state);

        Assert.Equal(expected, exception);
    }

    [Fact]
    public void When_Passing_Code_Then_Authorization_Code_Flow_Should_Be_Returned()
    {
        const string state = "state";

        var result =
            Assert.IsType<Option<AuthorizationFlow>.Result>(
                new[] { ResponseTypeNames.Code }.GetAuthorizationFlow(state));

        Assert.Equal(AuthorizationFlow.AuthorizationCodeFlow, result.Item);
    }

    [Fact]
    public void When_Passing_Id_Token_Then_Implicit_Flow_Should_Be_Returned()
    {
        const string state = "state";

        var result = Assert.IsType<Option<AuthorizationFlow>.Result>(
            new List<string> { ResponseTypeNames.IdToken }.GetAuthorizationFlow(state));

        Assert.Equal(AuthorizationFlow.ImplicitFlow, result.Item);
    }

    [Fact]
    public void When_Passing_Code_And_Id_Token_Then_Hybrid_Flow_Should_Be_Returned()
    {
        const string state = "state";

        var result = Assert.IsType<Option<AuthorizationFlow>.Result>(
            new List<string> { ResponseTypeNames.IdToken, ResponseTypeNames.Code }.GetAuthorizationFlow(state));

        Assert.Equal(AuthorizationFlow.HybridFlow, result.Item);
    }
}
