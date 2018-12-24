﻿namespace SimpleIdentityServer.Core.Api.Jws.Actions
{
    using System.Collections.Generic;
    using SimpleAuth.Shared;

    public interface IJsonWebKeyEnricher
    {
        Dictionary<string, object> GetPublicKeyInformation(JsonWebKey jsonWebKey);
        Dictionary<string, object> GetJsonWebKeyInformation(JsonWebKey jsonWebKey);
    }
}