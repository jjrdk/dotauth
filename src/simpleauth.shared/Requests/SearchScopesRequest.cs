namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SearchScopesRequest
    {
        [DataMember(Name = "types")]
        public string[] ScopeTypes { get; set; }

        [DataMember(Name = "names")]
        public string[] ScopeNames { get; set; }

        [DataMember(Name = "start_index")]
        public int StartIndex { get; set; }

        [DataMember(Name = "count")]
        public int NbResults { get; set; } = int.MaxValue;

        [DataMember(Name = "order")]
        public bool Descending { get; set; }
    }
}
