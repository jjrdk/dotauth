namespace SimpleAuth.Manager.Client.Results
{
    using Shared;
    using Shared.Responses;

    public class GetConfigurationResult : BaseResponse
    {
	    public DiscoveryInformation Content { get; set; }
    }
}
