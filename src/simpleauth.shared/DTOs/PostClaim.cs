namespace SimpleAuth.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PostClaim
    {
        [DataMember(Name = ClaimNames.Type)]
        public string Type { get; set; }
        [DataMember(Name = ClaimNames.Value)]
        public string Value { get; set; }
    }
}