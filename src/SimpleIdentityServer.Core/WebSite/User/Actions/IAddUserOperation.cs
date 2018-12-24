namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    public interface IAddUserOperation
    {
        Task<bool> Execute(ResourceOwner resourceOwner, Uri scimBaseUrl = null);
    }
}