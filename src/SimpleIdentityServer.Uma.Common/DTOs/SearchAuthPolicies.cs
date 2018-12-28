namespace SimpleAuth.Uma.Shared.DTOs
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SearchAuthPolicies
    {
        [DataMember(Name = SearchAuthPolicyNames.Ids)]
        public IEnumerable<string> Ids { get; set; }

        [DataMember(Name = SearchAuthPolicyNames.ResourceIds)]
        public IEnumerable<string> ResourceIds { get; set; }

        [DataMember(Name = SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [DataMember(Name = SearchResponseNames.TotalResults)]
        public int TotalResults { get; set; }
    }
}
