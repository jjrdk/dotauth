namespace SimpleAuth.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PostClaimToken
    {
        [DataMember(Name = "format")]
        public string Format { get; set; }
        [DataMember(Name = "token")]
        public string Token { get; set; }
    }
}