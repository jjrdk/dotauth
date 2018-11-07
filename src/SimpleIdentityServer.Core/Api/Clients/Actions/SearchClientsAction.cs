namespace SimpleIdentityServer.Core.Api.Clients.Actions
{
    using System;
    using System.Threading.Tasks;
    using Shared.Parameters;
    using Shared.Repositories;
    using Shared.Results;

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
