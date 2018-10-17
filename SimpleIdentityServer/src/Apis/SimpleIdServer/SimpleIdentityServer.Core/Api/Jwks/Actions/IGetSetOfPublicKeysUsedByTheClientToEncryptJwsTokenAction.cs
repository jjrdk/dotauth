namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction
    {
        Task<List<Dictionary<string, object>>> Execute();
    }
}