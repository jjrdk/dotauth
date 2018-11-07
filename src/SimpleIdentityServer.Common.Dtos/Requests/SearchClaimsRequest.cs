namespace SimpleIdentityServer.Shared.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class SearchClaimsRequest
    {
        [DataMember(Name = Constants.SearchClaimNames.Codes)]
        public IEnumerable<string> Codes { get; set; }
        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }
        [DataMember(Name = Constants.SearchScopeNames.Order)]
        public OrderRequest Order { get; set; }
    }
}
