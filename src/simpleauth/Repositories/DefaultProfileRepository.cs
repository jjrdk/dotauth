namespace SimpleAuth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Shared.Models;
    using Shared.Parameters;
    using Shared.Repositories;

    internal sealed class DefaultProfileRepository : IProfileRepository
    {
        public readonly List<ResourceOwnerProfile> _profiles;

        public DefaultProfileRepository(IReadOnlyCollection<ResourceOwnerProfile> profiles = null)
        {
            _profiles = profiles == null
                ? new List<ResourceOwnerProfile>()
                : profiles.ToList();
        }

        public Task<bool> Add(params ResourceOwnerProfile[] profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException(nameof(profiles));
            }

            foreach (var profile in profiles)
            {
                profile.CreateDateTime = DateTime.UtcNow;
            }

            _profiles.AddRange(profiles);
            return Task.FromResult(true);
        }

        public Task<ResourceOwnerProfile> Get(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var profile = _profiles.FirstOrDefault(p => p.Subject == subject);
            return profile == null 
                ? Task.FromResult((ResourceOwnerProfile)null) 
                : Task.FromResult(profile);
        }

        public Task<bool> Remove(IEnumerable<string> subjects)
        {
            if (subjects == null)
            {
                throw new ArgumentNullException(nameof(subjects));
            }

            var lstIndexToBeRemoved = _profiles.Where(p => subjects.Contains(p.Subject)).Select(p => _profiles.IndexOf(p)).OrderByDescending(p => p);
            foreach (var index in lstIndexToBeRemoved)
            {
                _profiles.RemoveAt(index);
            }

            return Task.FromResult(true);
        }

        public Task<IEnumerable<ResourceOwnerProfile>> Search(SearchProfileParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }


            IEnumerable<ResourceOwnerProfile> result = _profiles;
            if (parameter.ResourceOwnerIds != null && parameter.ResourceOwnerIds.Any())
            {
                result = result.Where(p => parameter.ResourceOwnerIds.Contains(p.ResourceOwnerId));
            }

            if (parameter.Issuers != null && parameter.Issuers.Any())
            {
                result = result.Where(p => parameter.Issuers.Contains(p.Issuer));
            }

            return Task.FromResult(result);
        }
    }
}
