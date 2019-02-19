namespace SimpleAuth.Shared.DTOs
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SearchResourceSet
    {
        [DataMember(Name = "ids")]
        public IEnumerable<string> Ids { get; set; }
        [DataMember(Name = "names")]
        public IEnumerable<string> Names { get; set; }
        [DataMember(Name = "types")]
        public IEnumerable<string> Types { get; set; }
        [DataMember(Name = "start_index")]
        public int StartIndex { get; set; }
        [DataMember(Name = "count")]
        public int TotalResults { get; set; }
    }
}
