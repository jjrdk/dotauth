namespace SimpleIdentityServer.Shared.Models
{
    public class ClientSecret
    {
        public ClientSecretTypes Type { get; set; }
        public string Value { get; set; }
    }
}