using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement.Client.Results
{
    using Common.Responses;
    using Core.Common;

    public class GetProfilesResult : BaseResponse
    {
        public IEnumerable<ProfileResponse> Content { get; set; }
    }
}