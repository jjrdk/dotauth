using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetProfilesResult : BaseResponse
    {
        public IEnumerable<ProfileResponse> Content { get; set; }
    }
}