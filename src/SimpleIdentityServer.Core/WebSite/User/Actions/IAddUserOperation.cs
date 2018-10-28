namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    using Parameters;
    using System;
    using System.Threading.Tasks;
    using Common.Models;

    public interface IAddUserOperation
    {
        Task<bool> Execute(ResourceOwner resourceOwner, Uri scimBaseUrl = null);
    }
}