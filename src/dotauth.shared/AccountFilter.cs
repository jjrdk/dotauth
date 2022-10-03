namespace DotAuth.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;

/// <summary>
/// Defines the account filtering.
/// </summary>
/// <seealso cref="IAccountFilter" />
public sealed class AccountFilter : IAccountFilter
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
        var allClaims = claims.ToArray();
        var accountFilterRules = new List<AccountFilterRuleResult>();
        var filters = await _filterStore.GetAll(cancellationToken).ConfigureAwait(false);

        foreach (var filter in filters)
        {
            var errorMessages = new List<string>();
            foreach (var rule in filter.Rules)
            {
                var claim = allClaims.FirstOrDefault(c => c.Type == rule.ClaimType);
                if (claim == null)
                {
                    errorMessages.Add(string.Format(ErrorMessages.TheClaimDoesntExist, rule.ClaimType));
                    continue;
                }

                switch (rule.Operation)
                {
                    case ComparisonOperations.Equal:
                        if (rule.ClaimValue != claim.Value)
                        {
                            errorMessages.Add(
                                string.Format(
                                    ErrorMessages.TheFilterEqualsIsWrong,
                                    claim.Type,
                                    rule.ClaimValue));
                        }

                        break;
                    case ComparisonOperations.NotEqual:
                        if (rule.ClaimValue == claim.Value)
                        {
                            errorMessages.Add(
                                string.Format(
                                    ErrorMessages.TheFilterNotEqualsIsWrong,
                                    claim.Type,
                                    rule.ClaimValue));
                        }

                        break;
                    case ComparisonOperations.RegularExpression:
                        var regex = new Regex(rule.ClaimValue);
                        if (!regex.IsMatch(claim.Value))
                        {
                            errorMessages.Add(
                                string.Format(
                                    ErrorMessages.TheFilterRegexIsWrong,
                                    claim.Type,
                                    rule.ClaimValue));
                        }

                        break;
                }
            }

            var accountFilterRule = new AccountFilterRuleResult(filter.Name, errorMessages.Count == 0, errorMessages.ToArray());
            accountFilterRules.Add(accountFilterRule);
        }

        return !accountFilterRules.Any()
            ? new AccountFilterResult(true)
            : new AccountFilterResult(accountFilterRules.Any(u => u.IsValid), accountFilterRules.ToArray());
    }
}