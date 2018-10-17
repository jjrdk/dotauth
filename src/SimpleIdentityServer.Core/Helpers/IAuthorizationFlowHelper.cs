namespace SimpleIdentityServer.Core.Helpers
{
    using System.Collections.Generic;
    using Api.Authorization;
    using Common.Models;

    public interface IAuthorizationFlowHelper
    {
        AuthorizationFlow GetAuthorizationFlow(ICollection<ResponseType> responseTypes, string state);
    }
}