namespace SimpleAuth.Validators
{
    using System;
    using System.Collections.Generic;
    using Shared.Models;

    public interface IClientValidator
    {
        IEnumerable<Uri> GetRedirectionUrls(Client client, params Uri[] urls);
        bool CheckGrantTypes(Client client, params GrantType[] grantTypes);
        bool CheckResponseTypes(Client client, params ResponseType[] responseTypes);
        bool CheckPkce(Client client, string codeVerifier, AuthorizationCode code);
    }
}