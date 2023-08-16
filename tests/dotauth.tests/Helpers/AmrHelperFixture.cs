namespace DotAuth.Tests.Helpers;

using System;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using Xunit;

public sealed class AmrHelperFixture
{
    [Fact]
    public void When_No_Amr_Then_Exception_Is_Thrown()
    {
        var exception = Assert.IsType<Option<string>.Error>(Array.Empty<string>().GetAmr(new[] { "pwd" }));

        Assert.Equal(ErrorCodes.InternalError, exception.Details.Title);
        Assert.Equal(Strings.NoActiveAmr, exception.Details.Detail);
    }

    [Fact]
    public void When_Amr_Does_Not_Exist_Then_Exception_Is_Thrown()
    {
        var exception = Assert.IsType<Option<string>.Error>(new[] { "invalid" }.GetAmr(new[] { "pwd" }));

        Assert.Equal(ErrorCodes.InternalError, exception.Details.Title);
        Assert.Equal(string.Format(Strings.TheAmrDoesntExist, "pwd"), exception.Details.Detail);
    }

    [Fact]
    public void When_Amr_Does_Not_Exist_Then_Default_One_Is_Returned()
    {
        var amr = Assert.IsType<Option<string>.Result>(new[] { "pwd" }.GetAmr(new[] { "invalid" }));

        Assert.Equal("pwd", amr.Item);
    }

    [Fact]
    public void When_Amr_Exists_Then_Same_Amr_Is_Returned()
    {
        var amr = Assert.IsType<Option<string>.Result>(new[] { "amr" }.GetAmr(new[] { "amr" }));

        Assert.Equal("amr", amr.Item);
    }
}
