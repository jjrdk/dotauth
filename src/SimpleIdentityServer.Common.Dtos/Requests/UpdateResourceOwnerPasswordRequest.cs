namespace SimpleIdentityServer.Shared.Requests
{
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class UpdateResourceOwnerPasswordRequest
    {
        [DataMember(Name = SharedConstants.ResourceOwnerResponseNames.Login)]
        public string Login { get; set; }
        [DataMember(Name = SharedConstants.ResourceOwnerResponseNames.Password)]
        public string Password { get; set; }
    }
}