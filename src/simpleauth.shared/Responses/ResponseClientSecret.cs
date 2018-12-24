namespace SimpleIdentityServer.Shared.Responses
{
    using System.Runtime.Serialization;
    using Models;

    [DataContract]
    public class ResponseClientSecret
    {
        [DataMember(Name = SharedConstants.ClientSecretNames.Type)]
        public ClientSecretTypes Type { get; set; }
        [DataMember(Name = SharedConstants.ClientSecretNames.Value)]
        public string Value { get; set; }
    }
}