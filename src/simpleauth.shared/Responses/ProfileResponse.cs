namespace SimpleAuth.Shared.Responses
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ProfileResponse
    {
        [DataMember(Name = UserManagementConstants.LinkProfileRequestNames.UserId)]
        public string UserId { get; set; }
        [DataMember(Name = UserManagementConstants.LinkProfileRequestNames.Issuer)]
        public string Issuer { get; set; }
        [DataMember(Name = UserManagementConstants.LinkProfileResponseNames.CreateDatetime)]
        public DateTime CreateDateTime { get; set; }
        [DataMember(Name = UserManagementConstants.LinkProfileResponseNames.UpdateDatetime)]
        public DateTime UpdateTime { get; set; }
    }
}
