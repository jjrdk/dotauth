namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SearchClaimsRequest
    {
        [DataMember(Name = "codes")]
        public string[] Codes { get; set; }
        [DataMember(Name = "start_index")]
        public int StartIndex { get; set; }
        [DataMember(Name = "count")]
        public int NbResults { get; set; }
        [DataMember(Name = "order")]
        public bool Descending { get; set; }
    }
}
