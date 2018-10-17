namespace SimpleIdentityServer.Core.Factories
{
    using Results;

    public interface IActionResultFactory
    {
        ActionResult CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
        ActionResult CreateAnEmptyActionResultWithRedirection();
        ActionResult CreateAnEmptyActionResultWithOutput();
        ActionResult CreateAnEmptyActionResultWithNoEffect();
    }
}