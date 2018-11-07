using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    using Shared;

    [DataContract]
    public class SearchScopesRequest
    {
        [DataMember(Name = Constants.SearchScopeNames.ScopeTypes)]
        public IList<int> ScopeTypes { get; set; }

        [DataMember(Name = Constants.SearchScopeNames.ScopeNames)]
        public IEnumerable<string> ScopeNames { get; set; }

        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }

        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }

        [DataMember(Name = Constants.SearchScopeNames.Order)]
        public OrderRequest Order { get; set; }
    }
}
