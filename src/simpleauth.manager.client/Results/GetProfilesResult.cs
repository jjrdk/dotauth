namespace SimpleAuth.Manager.Client.Results
{
    using System.Collections.Generic;
    using Shared;
    using Shared.Responses;

    public class GetProfilesResult : BaseResponse
    {
        public IEnumerable<ProfileResponse> Content { get; set; }
    }
}