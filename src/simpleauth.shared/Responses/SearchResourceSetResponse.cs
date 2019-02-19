namespace SimpleAuth.Shared.Responses
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SearchResourceSetResponse
    {
        [DataMember(Name = "content")]
        public IEnumerable<ResourceSetResponse> Content { get; set; }
        [DataMember(Name = "count")]
        public int TotalResults { get; set; }
        [DataMember(Name = "start_index")]
        public int StartIndex { get; set; }
    }
}
