namespace SimpleIdentityServer.Shared.Requests
{
    using System.Runtime.Serialization;
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