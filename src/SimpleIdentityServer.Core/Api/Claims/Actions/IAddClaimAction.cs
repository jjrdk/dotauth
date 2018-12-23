namespace SimpleIdentityServer.Core.Api.Claims.Actions
{
    using System.Threading.Tasks;
    using Shared.Parameters;

    public interface IAddClaimAction
    {
        Task<bool> Execute(AddClaimParameter request);
    }
}