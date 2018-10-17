using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement.Client.Results
{
    using Core.Common;
    using Responses;

    public class GetProfilesResult : BaseResponse
    {
        public IEnumerable<ProfileResponse> Content { get; set; }
    }
}