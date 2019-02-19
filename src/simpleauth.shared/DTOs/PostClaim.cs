namespace SimpleAuth.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PostClaim
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}