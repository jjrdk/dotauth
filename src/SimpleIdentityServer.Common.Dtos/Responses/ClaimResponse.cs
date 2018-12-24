namespace SimpleIdentityServer.Shared.Responses
{
    using System;
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class ClaimResponse
    {
        [DataMember(Name = SharedConstants.ClaimResponseNames.Code)]
        public string Code { get; set; }

        [DataMember(Name = SharedConstants.ClaimResponseNames.IsIdentifier)]
        public bool IsIdentifier  { get; set; }

        [DataMember(Name = SharedConstants.ClaimResponseNames.CreateDateTime)]
        public DateTime CreateDateTime { get; set; }

        [DataMember(Name = SharedConstants.ClaimResponseNames.UpdateDateTime)]
        public DateTime UpdateDateTime { get; set; }
    }
}
