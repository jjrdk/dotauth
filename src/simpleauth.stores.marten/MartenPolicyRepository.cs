namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MartenPolicyRepository : IPolicyRepository
    {
        private readonly Func<IDocumentSession> _sessionFactory;

        public MartenPolicyRepository(Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public async Task<GenericResult<Policy>> Search(
            SearchAuthPolicies parameter,
            CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var results = await session.Query<Policy>()
                    .Where(
                        x => x.Id.IsOneOf(parameter.Ids) || x.ResourceSetIds.Any(r => r.IsOneOf(parameter.ResourceIds)))
                    .Skip(parameter.StartIndex)
                    .Take(parameter.TotalResults)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                return new GenericResult<Policy>
                {
                    Content = results.ToArray(),
                    StartIndex = parameter.StartIndex,
                    TotalResults = results.Count
                };
            }
        }

        public async Task<Policy[]> GetAll(CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var policies = await session.Query<Policy>()
                    .ToListAsync(token: cancellationToken)
                    .ConfigureAwait(false);
                return policies.ToArray();
            }
        }

        public async Task<Policy> Get(string id, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                var policy = await session.LoadAsync<Policy>(id, cancellationToken).ConfigureAwait(false);
                return policy;
            }
        }

        public async Task<bool> Add(Policy policy, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Store(policy);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        public async Task<bool> Delete(string id, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Delete<Policy>(id);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        public async Task<bool> Update(Policy policy, CancellationToken cancellationToken = default)
        {
            using (var session = _sessionFactory())
            {
                session.Update(policy);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }
    }
}