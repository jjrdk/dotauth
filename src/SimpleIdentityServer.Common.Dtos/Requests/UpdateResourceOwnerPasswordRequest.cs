using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    using Shared;

    [DataContract]
    public class UpdateResourceOwnerPasswordRequest
    {
        [DataMember(Name = Constants.ResourceOwnerResponseNames.Login)]
        public string Login { get; set; }
        [DataMember(Name = Constants.ResourceOwnerResponseNames.Password)]
        public string Password { get; set; }
    }
}