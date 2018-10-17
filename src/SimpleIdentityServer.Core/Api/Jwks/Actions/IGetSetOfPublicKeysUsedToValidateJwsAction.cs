namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGetSetOfPublicKeysUsedToValidateJwsAction
    {
        Task<List<Dictionary<string, object>>> Execute();
    }
}