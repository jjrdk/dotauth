namespace SimpleAuth.Api.PolicyController.Actions
{
    using Parameters;
    using Repositories;
    using Shared.Models;
    using System;
    using System.Threading.Tasks;

    internal sealed class SearchAuthPoliciesAction
    {
        private readonly IPolicyRepository _policyRepository;

        public SearchAuthPoliciesAction(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        public Task<SearchAuthPoliciesResult> Execute(SearchAuthPoliciesParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _policyRepository.Search(parameter);
        }
    }
}
