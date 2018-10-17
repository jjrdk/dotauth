namespace SimpleIdentityServer.Core.Validators
{
    using System.Collections.Generic;
    using Common.Models;

    public interface IClientValidator
    {
        IEnumerable<string> GetRedirectionUrls(Client client, params string[] urls);
        bool CheckGrantTypes(Client client, params GrantType[] grantTypes);
        bool CheckResponseTypes(Client client, params ResponseType[] responseTypes);
        bool CheckPkce(Client client, string codeVerifier, AuthorizationCode code);
    }
}