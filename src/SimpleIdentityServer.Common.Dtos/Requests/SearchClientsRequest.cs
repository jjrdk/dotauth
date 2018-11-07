using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    using Shared;

    [DataContract]
    public class SearchClientsRequest
    {
        [DataMember(Name = Constants.SearchClientNames.ClientNames)]
        public IEnumerable<string> ClientNames { get; set; }
        [DataMember(Name = Constants.SearchClientNames.ClientIds)]
        public IEnumerable<string> ClientIds { get; set; }
        [DataMember(Name = Constants.SearchClientNames.ClientTypes)]
        public IEnumerable<int> ClientTypes { get; set; }
        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }
        [DataMember(Name = Constants.SearchClientNames.Order)]
        public OrderRequest Order { get; set; }
    }
}
