namespace SimpleIdentityServer.Shared.Requests
{
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class AddClaimRequest
    {
        [DataMember(Name = Constants.ClaimResponseNames.Code)]
        public string Code { get; set; }
        [DataMember(Name = Constants.ClaimResponseNames.IsIdentifier)]
        public bool IsIdentifier { get; set; }
    }
}
