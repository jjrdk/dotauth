namespace SimpleIdentityServer.Shared.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class SearchClientsRequest
    {
        [DataMember(Name = SharedConstants.SearchClientNames.ClientNames)]
        public IEnumerable<string> ClientNames { get; set; }
        [DataMember(Name = SharedConstants.SearchClientNames.ClientIds)]
        public IEnumerable<string> ClientIds { get; set; }
        [DataMember(Name = SharedConstants.SearchClientNames.ClientTypes)]
        public IEnumerable<int> ClientTypes { get; set; }
        [DataMember(Name = SharedConstants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
        [DataMember(Name = SharedConstants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }
        [DataMember(Name = SharedConstants.SearchClientNames.Order)]
        public OrderRequest Order { get; set; }
    }
}
