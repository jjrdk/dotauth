using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Responses
{
    using Shared;

    [DataContract]
    public class ConfigurationResponse
    {
        [DataMember(Name = Constants.ConfigurationResponseNames.ClientsEndpoint)]
        public string ClientsEndpoint { get; set; }
        [DataMember(Name = Constants.ConfigurationResponseNames.JweEndpoint)]
        public string JweEndpoint { get; set; }
        [DataMember(Name = Constants.ConfigurationResponseNames.JwsEndpoint)]
        public string JwsEndpoint { get; set; }
        [DataMember(Name = Constants.ConfigurationResponseNames.ManageEndpoint)]
        public string ManageEndpoint { get; set; }
        [DataMember(Name = Constants.ConfigurationResponseNames.ResourceOwnersEndpoint)]
        public string ResourceOwnersEndpoint { get; set; }
        [DataMember(Name = Constants.ConfigurationResponseNames.ScopesEndpoint)]
        public string ScopesEndpoint { get; set; }
        [DataMember(Name = Constants.ConfigurationResponseNames.ClaimsEndpoint)]
        public string ClaimsEndpoint { get; set; }
    }
}
