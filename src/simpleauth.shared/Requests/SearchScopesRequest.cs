namespace SimpleIdentityServer.Shared.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class SearchScopesRequest
    {
        [DataMember(Name = SharedConstants.SearchScopeNames.ScopeTypes)]
        public IList<int> ScopeTypes { get; set; }

        [DataMember(Name = SharedConstants.SearchScopeNames.ScopeNames)]
        public IEnumerable<string> ScopeNames { get; set; }

        [DataMember(Name = SharedConstants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [DataMember(Name = SharedConstants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }

        [DataMember(Name = SharedConstants.SearchScopeNames.Order)]
        public OrderRequest Order { get; set; }
    }
}
