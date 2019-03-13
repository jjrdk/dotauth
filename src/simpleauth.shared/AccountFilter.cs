namespace SimpleAuth.Shared
{
    using Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the account filtering.
    /// </summary>
    /// <seealso cref="SimpleAuth.Shared.IAccountFilter" />
    public class AccountFilter : IAccountFilter
    {
        private readonly IFilterStore _filterStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountFilter"/> class.
        /// </summary>
        /// <param name="filterStore">The filter store.</param>
        public AccountFilter(IFilterStore filterStore)
        {
            _filterStore = filterStore;
        }

        /// <summary>
        /// Checks the specified claims to determine whether the account should be filtered.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">claims</exception>
        public async Task<AccountFilterResult> Check(IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var accountFilterRules = new List<AccountFilterRuleResult>();
            var filters = await _filterStore.GetAll(cancellationToken).ConfigureAwait(false);
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var accountFilterRule = new AccountFilterRuleResult(filter.Name);
                    var errorMessages = new List<string>();
                    if (filter.Rules != null)
                    {
                        foreach (var rule in filter.Rules)
                        {
                            var claim = claims.FirstOrDefault(c => c.Type == rule.ClaimType);
                            if (claim == null)
                            {
                                errorMessages.Add($"the claim '{rule.ClaimType}' doesn't exist");
                                continue;
                            }

                            switch (rule.Operation)
                            {
                                case ComparisonOperations.Equal:
                                    if (rule.ClaimValue != claim.Value)
                                    {
                                        errorMessages.Add($"the filter claims['{claim.Type}'] == '{rule.ClaimValue}' is wrong");
                                    }
                                    break;
                                case ComparisonOperations.NotEqual:
                                    if (rule.ClaimValue == claim.Value)
                                    {
                                        errorMessages.Add($"the filter claims['{claim.Type}'] != '{rule.ClaimValue}' is wrong");
                                    }
                                    break;
                                case ComparisonOperations.RegularExpression:
                                    var regex = new Regex(rule.ClaimValue);
                                    if (!regex.IsMatch(claim.Value))
                                    {
                                        errorMessages.Add($"the filter claims['{claim.Type}'] match regular expression {rule.ClaimValue} is wrong");
                                    }
                                    break;
                            }
                        }
                    }

                    accountFilterRule.ErrorMessages.AddRange(errorMessages);
                    accountFilterRule.IsValid = !errorMessages.Any();
                    accountFilterRules.Add(accountFilterRule);
                }
            }

            if (!accountFilterRules.Any())
            {
                return new AccountFilterResult
                {
                    IsValid = true
                };
            }

            return new AccountFilterResult
            {
                AccountFilterRules = accountFilterRules.ToArray(),
                IsValid = accountFilterRules.Any(u => u.IsValid)
            };
        }
    }
}
