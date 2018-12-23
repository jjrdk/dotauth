namespace SimpleIdentityServer.Shared.Responses
{
    using System.Runtime.Serialization;
    using Models;

    [DataContract]
    public class ResponseClientSecret
    {
        [DataMember(Name = Constants.ClientSecretNames.Type)]
        public ClientSecretTypes Type { get; set; }
        [DataMember(Name = Constants.ClientSecretNames.Value)]
        public string Value { get; set; }
    }
}