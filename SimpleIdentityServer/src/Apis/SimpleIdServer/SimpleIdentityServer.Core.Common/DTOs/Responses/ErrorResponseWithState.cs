using System.Runtime.Serialization;

namespace SimpleIdentityServer.Core.Common.DTOs.Responses
{
    using SimpleIdentityServer.Common.Dtos.Responses;

    [DataContract]
    public class ErrorResponseWithState : ErrorResponse
    {
        [DataMember(Name = ErrorResponseWithStateNames.State)]
        public string State { get; set; }
    }
}
