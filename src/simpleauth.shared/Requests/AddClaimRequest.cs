namespace SimpleIdentityServer.Shared.Requests
{
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class AddClaimRequest
    {
        [DataMember(Name = SharedConstants.ClaimResponseNames.Code)]
        public string Code { get; set; }
        [DataMember(Name = SharedConstants.ClaimResponseNames.IsIdentifier)]
        public bool IsIdentifier { get; set; }
    }
}
