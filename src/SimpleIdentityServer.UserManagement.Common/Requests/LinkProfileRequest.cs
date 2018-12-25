namespace SimpleIdentityServer.UserManagement.Common.Requests
{
    using System.Runtime.Serialization;
    using Common;

    [DataContract]
    public sealed class LinkProfileRequest
    {
        [DataMember(Name = UserManagementConstants.LinkProfileRequestNames.UserId)]
        public string UserId { get; set; }
        [DataMember(Name = UserManagementConstants.LinkProfileRequestNames.Issuer)]
        public string Issuer { get; set; }
        [DataMember(Name = UserManagementConstants.LinkProfileRequestNames.Force)]
        public bool Force { get; set; }
    }
}
