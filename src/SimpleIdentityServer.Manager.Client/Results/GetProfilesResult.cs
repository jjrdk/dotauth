namespace SimpleIdentityServer.Manager.Client.Results
{
    using System.Collections.Generic;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetProfilesResult : BaseResponse
    {
        public IEnumerable<ProfileResponse> Content { get; set; }
    }
}