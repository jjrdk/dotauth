namespace DotAuth.Uma.Web.Tests.StepDefinitions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Reqnroll;

[Binding]
public class UmaBearerChallengeContextSteps
{
    private UmaBearerChallengeContext? _ctx;

    [Given("a UmaBearerChallengeContext is constructed")]
    public void GivenContextConstructed()
    {
        var httpCtx = new DefaultHttpContext();
        var scheme = new AuthenticationScheme(
            UmaBearerDefaults.AuthenticationScheme,
            null,
            typeof(UmaBearerHandler));
        _ctx = new UmaBearerChallengeContext(httpCtx, scheme, new UmaBearerOptions(), new AuthenticationProperties());
    }

    [When("TicketId is set to {string}")]
    public void WhenTicketIdIsSetTo(string value) => _ctx!.TicketId = value;

    [When("AsUri is set to {string}")]
    public void WhenAsUriIsSetTo(string value) => _ctx!.AsUri = value;

    [When("HandleResponse is called")]
    public void WhenHandleResponseIsCalled() => _ctx!.HandleResponse();

    [Then("reading TicketId returns {string}")]
    public void ThenTicketIdReturns(string expected) =>
        Assert.Equal(expected, _ctx!.TicketId);

    [Then("reading AsUri returns {string}")]
    public void ThenAsUriReturns(string expected) =>
        Assert.Equal(expected, _ctx!.AsUri);

    [Then("Handled is true")]
    public void ThenHandledIsTrue() =>
        Assert.True(_ctx!.Handled);
}
