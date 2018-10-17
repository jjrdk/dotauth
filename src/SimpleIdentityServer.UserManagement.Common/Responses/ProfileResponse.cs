namespace SimpleIdentityServer.UserManagement.Responses
{
    using System;
    using System.Runtime.Serialization;
    using Common;

    [DataContract]
    public class ProfileResponse
    {
        [DataMember(Name = Constants.LinkProfileRequestNames.UserId)]
        public string UserId { get; set; }
        [DataMember(Name = Constants.LinkProfileRequestNames.Issuer)]
        public string Issuer { get; set; }
        [DataMember(Name = Constants.LinkProfileResponseNames.CreateDatetime)]
        public DateTime CreateDateTime { get; set; }
        [DataMember(Name = Constants.LinkProfileResponseNames.UpdateDatetime)]
        public DateTime UpdateTime { get; set; }
    }
}
