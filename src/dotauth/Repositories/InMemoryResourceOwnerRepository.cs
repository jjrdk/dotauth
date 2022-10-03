namespace DotAuth.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;

/// <summary>
/// Defines the in-memory resource owner repository.
/// </summary>
/// <seealso cref="IResourceOwnerRepository" />
internal sealed class InMemoryResourceOwnerRepository : IResourceOwnerRepository
{
    private readonly string _salt;
    private readonly List<ResourceOwner> _users;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryResourceOwnerRepository"/> class.
    /// </summary>
    /// <param name="salt">The salt.</param>
    /// <param name="users">The users.</param>
    public InMemoryResourceOwnerRepository(string salt, IReadOnlyCollection<ResourceOwner>? users = null)
    {
        _salt = salt;
        _users = users == null
            ? new List<ResourceOwner>()
            : users.ToList();
    }

    /// <inheritdoc />
    public Task<bool> SetPassword(string subject, string password, CancellationToken cancellationToken)
    {
        var user = _users.FirstOrDefault(x => x.Subject == subject);
        if (user == null)
        {
            return Task.FromResult(false);
        }

        user.Password = password.ToSha256Hash(_salt);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> Delete(string subject, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentNullException(nameof(subject));
        }

        var user = _users.FirstOrDefault(u => u.Subject == subject);
        if (user == null)
        {
            return Task.FromResult(false);
        }

        _users.Remove(user);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<ResourceOwner[]> GetAll(CancellationToken cancellationToken)
    {
        var res = _users.ToArray();
        return Task.FromResult(res);
    }

    /// <inheritdoc />
    public Task<ResourceOwner?> Get(string id, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Subject == id);
        return Task.FromResult(user);
    }

    /// <inheritdoc />
    public Task<ResourceOwner?> Get(ExternalAccountLink externalAccount, CancellationToken cancellationToken)
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

    /// <inheritdoc />
    public Task<ResourceOwner?> Get(string id, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        var user = _users.FirstOrDefault(u => u.Subject == id && u.Password == password.ToSha256Hash(_salt));
        return Task.FromResult(user);
    }

    /// <inheritdoc />
    public Task<ResourceOwner?> GetResourceOwnerByClaim(
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
        return Task.FromResult(user);
    }

    /// <inheritdoc />
    public Task<bool> Insert(ResourceOwner resourceOwner, CancellationToken cancellationToken = default)
    {
        if (resourceOwner == null)
        {
            throw new ArgumentNullException(nameof(resourceOwner));
        }

        if (resourceOwner.Password == null)
        {
            return Task.FromResult(false);
        }

        resourceOwner.Password = resourceOwner.Password.ToSha256Hash(_salt);
        resourceOwner.CreateDateTime = DateTimeOffset.UtcNow;
        _users.Add(resourceOwner);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<PagedResult<ResourceOwner>> Search(
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
            result = result.Where(r => parameter.Subjects.Any(s => r.Subject != null && r.Subject.Contains(s)));
        }

        var nbResult = result.Count();

        result = parameter.Descending
            ? result.OrderByDescending(c => c.UpdateDateTime)
            : result.OrderBy(x => x.UpdateDateTime);

        if (parameter.NbResults > 0)
        {
            result = result.Skip(parameter.StartIndex).Take(parameter.NbResults);
        }

        return Task.FromResult(
            new PagedResult<ResourceOwner>
            {
                Content = result.ToArray(),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
    }

    /// <inheritdoc />
    public Task<Option> Update(ResourceOwner resourceOwner, CancellationToken cancellationToken)
    {
        if (resourceOwner == null)
        {
            throw new ArgumentNullException(nameof(resourceOwner));
        }

        var user = _users.FirstOrDefault(u => u.Subject == resourceOwner.Subject);
        if (user == null)
        {
            return Task.FromResult<Option>(new Option.Error(new ErrorDetails
            {
                Title = ErrorCodes.InternalError,
                Detail = Strings.TheRoDoesntExist,
                Status = HttpStatusCode.InternalServerError
            }));
        }

        user.IsLocalAccount = resourceOwner.IsLocalAccount;
        user.TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication;
        user.UpdateDateTime = DateTimeOffset.UtcNow;
        user.Claims = resourceOwner.Claims;
        return Task.FromResult<Option>(new Option.Success());
    }
}