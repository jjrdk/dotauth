namespace SimpleIdentityServer.Shared.Models
{
    public enum ClientSecretTypes
    {
        SharedSecret= 0,
        X509Thumbprint = 1,
        X509Name = 2
    }
}