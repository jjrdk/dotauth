namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private Option<PagedResult<ResourceSetDescription>>.Result? _searchResults;

    [When(@"searching by term (.+)")]
    public async Task WhenSearchingByTerm(string term)
    {
        var searchOption = await _umaClient.SearchResources(
            new SearchResourceSet { Terms = new[] { term } },
            token: _token.AccessToken);
        _searchResults = Assert.IsType<Option<PagedResult<ResourceSetDescription>>.Result>(searchOption);
    }

    [Then(@"returns (\d+) search results")]
    public void ThenReturnsSearchResults(int amount)
    {
        Assert.Equal(amount, _searchResults!.Item.Content.Length);
    }

    [When(@"searching by type (.+) and term (.+)")]
    public async Task WhenSearchingByTypeAndTerm(string type, string term)
    {
        var searchOption = await _umaClient.SearchResources(
            new SearchResourceSet
            {
                Terms = term.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                Types = type.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            },
            token: _token.AccessToken);
        _searchResults = Assert.IsType<Option<PagedResult<ResourceSetDescription>>.Result>(searchOption);
    }
}
