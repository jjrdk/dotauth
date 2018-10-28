using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Clients.Actions
{
    using SimpleIdentityServer.Core.Common.Results;

    public interface ISearchClientsAction
    {
        Task<SearchClientResult> Execute(SearchClientParameter parameter);
    }

    internal sealed class SearchClientsAction : ISearchClientsAction
    {
        private readonly IClientRepository _clientRepository;

        public SearchClientsAction(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public Task<SearchClientResult> Execute(SearchClientParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _clientRepository.Search(parameter);
        }
    }
}
