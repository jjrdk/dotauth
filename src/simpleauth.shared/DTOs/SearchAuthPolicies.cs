namespace SimpleAuth.Shared.DTOs
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class SearchAuthPolicies
    {
        [DataMember(Name = "ids")]
        public string[] Ids { get; set; } = Array.Empty<string>();

        [DataMember(Name = "resource_ids")]
        public string[] ResourceIds { get; set; } = Array.Empty<string>();

        [DataMember(Name = "start_index")]
        public int StartIndex { get; set; }

        [DataMember(Name = "count")]
        public int TotalResults { get; set; }
    }
}
