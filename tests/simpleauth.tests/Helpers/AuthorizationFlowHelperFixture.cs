namespace SimpleAuth.Tests.Helpers;

using Shared;
using SimpleAuth.Api.Authorization;
using System.Collections.Generic;
using SimpleAuth.Extensions;
using SimpleAuth.Properties;
using SimpleAuth.Shared.Errors;
using SimpleAuth.Shared.Models;
using Xunit;

public sealed class AuthorizationFlowHelperFixture
{
    [Fact]
    public void WhenPassingEmptyListOfResponseTypesThenErrorIsReturned()
    {
        const string state = "state";

        var exception = new List<string>().GetAuthorizationFlow(state) as Option<AuthorizationFlow>.Error;

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

        var result = new[] { ResponseTypeNames.Code }.GetAuthorizationFlow(state) as Option<AuthorizationFlow>.Result;

        Assert.Equal(AuthorizationFlow.AuthorizationCodeFlow, result.Item);
    }

    [Fact]
    public void When_Passing_Id_Token_Then_Implicit_Flow_Should_Be_Returned()
    {
        const string state = "state";

        var result =
            new List<string> { ResponseTypeNames.IdToken }.GetAuthorizationFlow(state) as
                Option<AuthorizationFlow>.Result;

        Assert.Equal(AuthorizationFlow.ImplicitFlow, result.Item);
    }

    [Fact]
    public void When_Passing_Code_And_Id_Token_Then_Hybrid_Flow_Should_Be_Returned()
    {
        const string state = "state";

        var result =
            new List<string> { ResponseTypeNames.IdToken, ResponseTypeNames.Code }.GetAuthorizationFlow(state) as
                Option<AuthorizationFlow>.Result;

        Assert.Equal(AuthorizationFlow.HybridFlow, result.Item);
    }
}