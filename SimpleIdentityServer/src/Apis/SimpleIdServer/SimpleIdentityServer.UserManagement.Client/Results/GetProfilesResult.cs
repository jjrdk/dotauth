using SimpleIdentityServer.UserManagement.Common.Responses;
using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement.Client.Results
{
    using Core.Common;

    public class GetProfilesResult : BaseResponse
    {
        public IEnumerable<ProfileResponse> Content { get; set; }
    }
}