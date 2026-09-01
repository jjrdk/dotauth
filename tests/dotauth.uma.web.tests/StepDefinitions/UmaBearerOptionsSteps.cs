namespace DotAuth.Uma.Web.Tests.StepDefinitions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Reqnroll;

[Binding]
public class UmaBearerOptionsSteps
{
    private UmaBearerOptions? _options;

    [Given("a new instance of UmaBearerOptions")]
    [Given("a UmaBearerOptions instance")]
    public void GivenANewUmaBearerOptions()
    {
        _options = new UmaBearerOptions();
    }

    [When("IdTokenHeader is set to {string}")]
    public void WhenIdTokenHeaderIsSetTo(string value) => _options!.IdTokenHeader = value;

    [When("ResourceIdParameters is set to {string}")]
    public void WhenResourceIdParametersIsSetTo(string csv) =>
        _options!.ResourceIdParameters = csv.Split(',');

    [When("ResourceSetIdFormat is set to {string}")]
    public void WhenResourceSetIdFormatIsSetTo(string value) => _options!.ResourceSetIdFormat = value;

    [When("Realm is set to {string}")]
    public void WhenRealmIsSetTo(string value) => _options!.Realm = value;

    [Then("IdTokenHeader equals {string}")]
    public void ThenIdTokenHeaderEquals(string expected) =>
        Assert.Equal(expected, _options!.IdTokenHeader);

    [Then("ResourceIdParameters is empty")]
    public void ThenResourceIdParametersIsEmpty() =>
        Assert.Empty(_options!.ResourceIdParameters);

    [Then("ResourceSetIdFormat is null")]
    public void ThenResourceSetIdFormatIsNull() =>
        Assert.Null(_options!.ResourceSetIdFormat);

    [Then("Realm is null")]
    public void ThenRealmIsNull() =>
        Assert.Null(_options!.Realm);

    [Then("ResourceIdParameters contains {string} and {string}")]
    public void ThenResourceIdParametersContains(string first, string second)
    {
        Assert.Contains(first, _options!.ResourceIdParameters);
        Assert.Contains(second, _options!.ResourceIdParameters);
    }

    [Then("ResourceSetIdFormat equals {string}")]
    public void ThenResourceSetIdFormatEquals(string expected) =>
        Assert.Equal(expected, _options!.ResourceSetIdFormat);

    [Then("Realm equals {string}")]
    public void ThenRealmEquals(string expected) =>
        Assert.Equal(expected, _options!.Realm);
}
