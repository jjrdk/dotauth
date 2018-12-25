namespace SimpleAuth.Api.Claims.Actions
{
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IGetClaimAction
    {
        Task<ClaimAggregate> Execute(string claimCode);
    }
}