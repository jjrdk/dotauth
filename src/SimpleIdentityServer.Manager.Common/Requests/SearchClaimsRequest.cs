using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    [DataContract]
    public class SearchClaimsRequest
    {
        [JsonProperty(Constants.SearchClaimNames.Codes)]
        [DataMember(Name = Constants.SearchClaimNames.Codes)]
        public IEnumerable<string> Codes { get; set; }
        [JsonProperty(Constants.SearchResponseNames.StartIndex)]
        [DataMember(Name = Constants.SearchResponseNames.StartIndex)]
        public int StartIndex { get; set; }
        [JsonProperty(Constants.SearchResponseNames.TotalResults)]
        [DataMember(Name = Constants.SearchResponseNames.TotalResults)]
        public int NbResults { get; set; }
        [JsonProperty(Constants.SearchScopeNames.Order)]
        [DataMember(Name = Constants.SearchScopeNames.Order)]
        public OrderRequest Order { get; set; }
    }
}
