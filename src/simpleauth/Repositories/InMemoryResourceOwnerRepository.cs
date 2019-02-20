namespace SimpleAuth.Repositories
{
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Results;
    using SimpleAuth.Shared.Requests;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Extensions;

    internal sealed class InMemoryResourceOwnerRepository : IResourceOwnerRepository
    {
        private readonly List<ResourceOwner> _users;

        public InMemoryResourceOwnerRepository(IReadOnlyCollection<ResourceOwner> users = null)
        {
            _users = users == null
                ? new List<ResourceOwner>()
                : users.ToList();
        }

        public Task<bool> Delete(string subject, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var user = _users.FirstOrDefault(u => u.Id == subject);
            if (user == null)
            {
                return Task.FromResult(false);
            }

            _users.Remove(user);
            return Task.FromResult(true);
        }

        public Task<ResourceOwner[]> GetAll(CancellationToken cancellationToken)
        {
            var res = _users.ToArray();
            return Task.FromResult(res);
        }

        public Task<ResourceOwner> Get(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id), "The parameter login is missing");
            }

            var user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }

        public Task<ResourceOwner> Get(ExternalAccountLink externalAccount, CancellationToken cancellationToken)
        {
            if (externalAccount == null)
            {
                throw new ArgumentNullException(nameof(externalAccount));
            }

            var user = _users.FirstOrDefault(
                u => u.ExternalLogins != null
                     && u.ExternalLogins.Any(
                         e => e.Subject == externalAccount.Subject && e.Issuer == externalAccount.Issuer));
            return Task.FromResult(user);
        }

        public Task<ResourceOwner> Get(string id, string password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var user = _users.FirstOrDefault(u => u.Id == id && u.Password == password.ToSha256Hash());
            if (user == null)
            {
                return Task.FromResult((ResourceOwner)null);
            }

            return Task.FromResult(user);
        }

        public Task<ResourceOwner> GetResourceOwnerByClaim(
            string key,
            string value,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            var user = _users.FirstOrDefault(u => u.Claims.Any(c => c.Type == key && c.Value == value));
            if (user == null)
            {
                return Task.FromResult((ResourceOwner)null);
            }

            return Task.FromResult(user);
        }

        public Task<bool> Insert(ResourceOwner resourceOwner, CancellationToken cancellationToken = default)
        {
            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            resourceOwner.Password = resourceOwner.Password.ToSha256Hash();
            resourceOwner.CreateDateTime = DateTime.UtcNow;
            _users.Add(resourceOwner);
            return Task.FromResult(true);
        }

        public Task<SearchResourceOwnerResult> Search(
            SearchResourceOwnersRequest parameter,
            CancellationToken cancellationToken = default)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<ResourceOwner> result = _users;
            if (parameter.Subjects != null)
            {
                result = result.Where(r => parameter.Subjects.Any(s => r.Id.Contains(s)));
            }

            var nbResult = result.Count();

            result = parameter.Descending
                ? result.OrderByDescending(c => c.UpdateDateTime)
                : result.OrderBy(x => x.UpdateDateTime);

            if (parameter.NbResults > 0)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.NbResults);
            }

            return Task.FromResult(new SearchResourceOwnerResult
            {
                Content = result,
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<bool> Update(ResourceOwner resourceOwner, CancellationToken cancellationToken = default)
        {
            if (resourceOwner == null)
            {
                throw new ArgumentNullException(nameof(resourceOwner));
            }

            var user = _users.FirstOrDefault(u => u.Id == resourceOwner.Id);
            if (user == null)
            {
                return Task.FromResult(false);
            }

            user.IsLocalAccount = resourceOwner.IsLocalAccount;
            user.Password = resourceOwner.Password.ToSha256Hash();
            user.TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication;
            user.UpdateDateTime = DateTime.UtcNow;
            user.Claims = resourceOwner.Claims;
            return Task.FromResult(true);
        }
    }
}
