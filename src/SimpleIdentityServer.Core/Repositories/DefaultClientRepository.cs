using SimpleIdentityServer.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Repositories
{
    using Shared.Models;
    using Shared.Parameters;
    using Shared.Repositories;
    using Shared.Results;

    internal sealed class DefaultClientRepository : IClientRepository, IClientStore
    {
        private readonly ICollection<Client> _clients;

        public DefaultClientRepository(IReadOnlyCollection<Client> clients)
        {
            _clients = clients == null
                ? new List<Client>()
                : clients.ToList();
        }

        public Task<bool> DeleteAsync(Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            var client = _clients.FirstOrDefault(c => c.ClientId == newClient.ClientId);
            if (client == null)
            {
                return Task.FromResult(false);
            }

            _clients.Remove(client);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<Client>> GetAllAsync()
        {
            return Task.FromResult((IEnumerable<Client>)_clients.Select(c => c.Copy()));
        }

        public Task<Client> GetById(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var res = _clients.FirstOrDefault(c => c.ClientId == clientId);
            if (res == null)
            {
                return Task.FromResult((Client)null);
            }
            
            return Task.FromResult(res.Copy());
        }

        public Task<bool> InsertAsync(Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            newClient.CreateDateTime = DateTime.UtcNow;
            _clients.Add(newClient.Copy());
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAllAsync()
        {
            _clients.Clear();
            return Task.FromResult(true);
        }

        public Task<SearchClientResult> Search(SearchClientParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }


            IEnumerable<Client> result = _clients;
            if (parameter.ClientIds != null && parameter.ClientIds.Any())
            {
                result = result.Where(c => parameter.ClientIds.Any(i => c.ClientId.Contains(i)));
            }

            if (parameter.ClientNames != null && parameter.ClientNames.Any())
            {
                result = result.Where(c => parameter.ClientNames.Any(n => c.ClientName.Contains(n)));
            }

            if (parameter.ClientTypes != null && parameter.ClientTypes.Any())
            {
                var clientTypes = parameter.ClientTypes.Select(t => (ApplicationTypes)t);
                result = result.Where(c => clientTypes.Contains(c.ApplicationType));
            }

            var nbResult = result.Count();
            if (parameter.Order != null)
            {
                switch (parameter.Order.Target)
                {
                    case "update_datetime":
                        switch (parameter.Order.Type)
                        {
                            case OrderTypes.Asc:
                                result = result.OrderBy(c => c.UpdateDateTime);
                                break;
                            case OrderTypes.Desc:
                                result = result.OrderByDescending(c => c.UpdateDateTime);
                                break;
                        }
                        break;
                }
            }
            else
            {
                result = result.OrderByDescending(c => c.UpdateDateTime);
            }

            if (parameter.IsPagingEnabled)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return Task.FromResult(new SearchClientResult
            {
                Content = result.Select(s => s.Copy()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<bool> UpdateAsync(Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            var client = _clients.FirstOrDefault(c => c.ClientId == newClient.ClientId);
            client.ClientName = newClient.ClientName;
            client.ClientUri = newClient.ClientUri;
            client.Contacts = newClient.Contacts;
            client.DefaultAcrValues = newClient.DefaultAcrValues;
            client.DefaultMaxAge = newClient.DefaultMaxAge;
            client.GrantTypes = newClient.GrantTypes;
            client.IdTokenEncryptedResponseAlg = newClient.IdTokenEncryptedResponseAlg;
            client.IdTokenEncryptedResponseEnc = newClient.IdTokenEncryptedResponseEnc;
            client.IdTokenSignedResponseAlg = newClient.IdTokenSignedResponseAlg;
            client.InitiateLoginUri = newClient.InitiateLoginUri;
            client.JsonWebKeys = newClient.JsonWebKeys;
            client.JwksUri = newClient.JwksUri;
            client.LogoUri = newClient.LogoUri;
            client.PolicyUri = newClient.PolicyUri;
            client.PostLogoutRedirectUris = newClient.PostLogoutRedirectUris;
            client.RedirectionUrls = newClient.RedirectionUrls;
            client.RequestObjectEncryptionAlg = newClient.RequestObjectEncryptionAlg;
            client.RequestObjectEncryptionEnc = newClient.RequestObjectEncryptionEnc;
            client.RequestObjectSigningAlg = newClient.RequestObjectSigningAlg;
            client.RequestUris = newClient.RequestUris;
            client.RequireAuthTime = newClient.RequireAuthTime;
            client.RequirePkce = newClient.RequirePkce;
            client.ResponseTypes = newClient.ResponseTypes;
            client.ScimProfile = newClient.ScimProfile;
            client.Secrets = newClient.Secrets;
            client.SectorIdentifierUri = newClient.SectorIdentifierUri;
            client.SubjectType = newClient.SubjectType;
            client.TokenEndPointAuthMethod = newClient.TokenEndPointAuthMethod;
            client.TokenEndPointAuthSigningAlg = newClient.TokenEndPointAuthSigningAlg;
            client.TosUri = newClient.TosUri;
            client.UpdateDateTime = DateTime.UtcNow;
            client.UserInfoEncryptedResponseAlg = newClient.UserInfoEncryptedResponseAlg;
            client.UserInfoEncryptedResponseEnc = newClient.UserInfoEncryptedResponseEnc;
            client.UserInfoSignedResponseAlg = newClient.UserInfoSignedResponseAlg;
            return Task.FromResult(true);
        }
    }
}
