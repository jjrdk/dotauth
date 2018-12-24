namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetConfigurationResult : BaseResponse
    {
	    public DiscoveryInformation Content { get; set; }
    }
}
