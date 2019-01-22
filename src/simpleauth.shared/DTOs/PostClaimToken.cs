namespace SimpleAuth.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PostClaimToken
    {
        [DataMember(Name = PostClaimTokenNames.Format)]
        public string Format { get; set; }
        [DataMember(Name = PostClaimTokenNames.Token)]
        public string Token { get; set; }
    }
}