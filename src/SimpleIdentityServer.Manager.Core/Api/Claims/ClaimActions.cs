using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Results;
using SimpleIdentityServer.Manager.Core.Api.Claims.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Claims
{
    public interface IClaimActions
    {
        Task<bool> Add(AddClaimParameter request);
        Task<bool> Delete(string claimCode);
        Task<ClaimAggregate> Get(string claimCode);
        Task<SearchClaimsResult> Search(SearchClaimsParameter parameter);
        Task<IEnumerable<ClaimAggregate>> GetAll();
    }

    internal sealed class ClaimActions : IClaimActions
    {
        private readonly IAddClaimAction _addClaimAction;
        private readonly IDeleteClaimAction _deleteClaimAction;
        private readonly IGetClaimAction _getClaimAction;
        private readonly ISearchClaimsAction _searchClaimsAction;
        private readonly IGetClaimsAction _getClaimsAction;

        public ClaimActions(IAddClaimAction addClaimAction, IDeleteClaimAction deleteClaimAction,
            IGetClaimAction getClaimAction, ISearchClaimsAction searchClaimsAction,
            IGetClaimsAction getClaimsAction)
        {
            _addClaimAction = addClaimAction;
            _deleteClaimAction = deleteClaimAction;
            _getClaimAction = getClaimAction;
            _searchClaimsAction = searchClaimsAction;
            _getClaimAction = getClaimAction;
            _getClaimsAction = getClaimsAction;
        }

        public Task<bool> Add(AddClaimParameter request)
        {
            return _addClaimAction.Execute(request);
        }

        public Task<bool> Delete(string claimCode)
        {
            return _deleteClaimAction.Execute(claimCode);
        }

        public Task<ClaimAggregate> Get(string claimCode)
        {
            return _getClaimAction.Execute(claimCode);
        }

        public Task<SearchClaimsResult> Search(SearchClaimsParameter parameter)
        {
            return _searchClaimsAction.Execute(parameter);
        }

        public Task<IEnumerable<ClaimAggregate>> GetAll()
        {
            return _getClaimsAction.Execute();
        }
    }
}
