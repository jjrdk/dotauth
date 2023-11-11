namespace DotAuth.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using NSubstitute;
using Xunit;

public sealed class AccountFilterFixture
{
    private readonly IFilterStore _filterRepositoryStub;
    private readonly IAccountFilter _accountFilter;

    public AccountFilterFixture()
    {
        _filterRepositoryStub = Substitute.For<IFilterStore>();
        _accountFilter = new AccountFilter(_filterRepositoryStub);
    }

    [Fact]
    public async Task When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _accountFilter.Check(null, CancellationToken.None))
            ;
    }

    [Fact]
    public async Task When_Claim_Does_Not_Exist_Then_Error_Is_Returned()
    {
        var filters = new[]
        {
            new Filter("name", new FilterRule("key", "val", ComparisonOperations.Equal))
        };
        _filterRepositoryStub.GetAll(Arg.Any<CancellationToken>()).Returns(filters);

        var result = await _accountFilter.Check(new List<Claim> {new("keyv", "valv")}, CancellationToken.None)
            ;

        Assert.False(result.IsValid);
        Assert.Single(result.AccountFilterRules);
        Assert.Equal("The claim 'key' doesn't exist", result.AccountFilterRules.First().ErrorMessages.First());
    }

    [Fact]
    public async Task When_Filter_Claim_Value_Equal_To_Val_Is_Wrong_Then_Error_Is_Returned()
    {
        var filters = new[]
        {
            new Filter("name", new FilterRule("key", "val", ComparisonOperations.Equal))
        };
        _filterRepositoryStub.GetAll(Arg.Any<CancellationToken>()).Returns(filters);

        var result = await _accountFilter.Check(new List<Claim> {new("key", "valv")}, CancellationToken.None)
            ;

        Assert.False(result.IsValid);
        Assert.Single(result.AccountFilterRules);
        Assert.Equal(
            "The filter claims['key'] == 'val' is wrong",
            result.AccountFilterRules.First().ErrorMessages.First());
    }

    [Fact]
    public async Task When_Filter_Claim_Value_Not_Equal_To_Val_Is_Wrong_Then_Error_Is_Returned()
    {
        var filters = new[]
        {
            new Filter("name", new FilterRule("key", "val", ComparisonOperations.NotEqual))
        };
        _filterRepositoryStub.GetAll(Arg.Any<CancellationToken>()).Returns(filters);

        var result = await _accountFilter.Check(new List<Claim> {new("key", "val")}, CancellationToken.None)
            ;

        Assert.False(result.IsValid);
        Assert.Single(result.AccountFilterRules);
        Assert.Equal(
            "The filter claims['key'] != 'val' is wrong",
            result.AccountFilterRules.First().ErrorMessages.First());
    }

    [Fact]
    public async Task When_Filter_Claim_Value_Does_Not_Match_Regular_Expression_Is_Wrong_Then_Error_Is_Returned()
    {
        var filters = new[]
        {
            new Filter("name", new FilterRule("key", "^[0-9]{1}$", ComparisonOperations.RegularExpression))
        };
        _filterRepositoryStub.GetAll(Arg.Any<CancellationToken>()).Returns(filters);

        var result = await _accountFilter.Check(new List<Claim> {new("key", "111")}, CancellationToken.None)
            ;

        Assert.False(result.IsValid);
        Assert.Single(result.AccountFilterRules);
        Assert.Equal(
            "The filter claims['key'] match regular expression ^[0-9]{1}$ is wrong",
            result.AccountFilterRules.First().ErrorMessages.First());
    }

    [Fact]
    public async Task When_Filter_Claim_Value_Equal_To_Val_Is_Correct_Then_True_Is_Returned()
    {
        var filters = new[]
        {
            new Filter("name", new FilterRule("key", "val", ComparisonOperations.Equal))
        };
        _filterRepositoryStub.GetAll(Arg.Any<CancellationToken>()).Returns(filters);

        var result = await _accountFilter.Check(new List<Claim> {new("key", "val")}, CancellationToken.None)
            ;

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task
        When_Filter_Claim_Value_Equal_To_Val_Is_Correct_And_Filter_Claim_Value_Different_To_Val_Is_Incorrect_Then_True_Is_Returned()
    {
        var filters = new[]
        {
            new Filter("1", new FilterRule("key", "val", ComparisonOperations.Equal)),
            new Filter("2", new FilterRule("key", "val", ComparisonOperations.NotEqual))
        };
        _filterRepositoryStub.GetAll(Arg.Any<CancellationToken>()).Returns(filters);

        var result = await _accountFilter.Check(new List<Claim> {new("key", "val")}, CancellationToken.None)
            ;

        Assert.True(result.IsValid);
    }
}
