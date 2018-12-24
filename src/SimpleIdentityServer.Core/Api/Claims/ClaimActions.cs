namespace SimpleIdentityServer.Core.Api.Claims
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Actions;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Parameters;
    using SimpleAuth.Shared.Results;

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
