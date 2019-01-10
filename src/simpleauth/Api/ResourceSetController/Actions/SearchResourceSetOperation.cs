namespace SimpleAuth.Api.ResourceSetController.Actions
{
    using System;
    using System.Threading.Tasks;
    using Parameters;
    using Repositories;
    using Shared.Models;

    internal sealed class SearchResourceSetOperation : ISearchResourceSetOperation
    {
        private readonly IResourceSetRepository _resourceSetRepository;

        public SearchResourceSetOperation(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }

        public Task<SearchResourceSetResult> Execute(SearchResourceSetParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _resourceSetRepository.Search(parameter);
        }
    }
}
