namespace SimpleAuth.Tests.Helpers;

using SimpleAuth.Extensions;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Errors;
using System;
using SimpleAuth.Properties;
using Xunit;

public sealed class AmrHelperFixture
{
    [Fact]
    public void When_No_Amr_Then_Exception_Is_Thrown()
    {
        var exception = Array.Empty<string>().GetAmr(new[] { "pwd" }) as Option<string>.Error;

        Assert.Equal(ErrorCodes.InternalError, exception.Details.Title);
        Assert.Equal(Strings.NoActiveAmr, exception.Details.Detail);
    }

    [Fact]
    public void When_Amr_Does_Not_Exist_Then_Exception_Is_Thrown()
    {
        var exception = new[] { "invalid" }.GetAmr(new[] { "pwd" }) as Option<string>.Error;

        Assert.Equal(ErrorCodes.InternalError, exception.Details.Title);
        Assert.Equal(string.Format(Strings.TheAmrDoesntExist, "pwd"), exception.Details.Detail);
    }

    [Fact]
    public void When_Amr_Does_Not_Exist_Then_Default_One_Is_Returned()
    {
        var amr = new[] { "pwd" }.GetAmr(new[] { "invalid" }) as Option<string>.Result;

        Assert.Equal("pwd", amr.Item);
    }

    [Fact]
    public void When_Amr_Exists_Then_Same_Amr_Is_Returned()
    {
        var amr = new[] { "amr" }.GetAmr(new[] { "amr" }) as Option<string>.Result;

        Assert.Equal("amr", amr.Item);
    }
}