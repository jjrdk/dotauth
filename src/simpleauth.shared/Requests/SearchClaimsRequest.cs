namespace SimpleAuth.Shared.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SearchClaimsRequest
    {
        [DataMember(Name = SharedConstants.SearchClaimNames.Codes)]
        public IEnumerable<string> Codes { get; set; }
        [DataMember(Name = SharedConstants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
        [DataMember(Name = SharedConstants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }
        [DataMember(Name = SharedConstants.SearchScopeNames.Order)]
        public OrderRequest Order { get; set; }
    }
}
