namespace SimpleAuth.Helpers
{
    using System.Collections.Generic;
    using Api.Authorization;
    using Shared.Requests;

    public interface IAuthorizationFlowHelper
    {
        AuthorizationFlow GetAuthorizationFlow(ICollection<string> responseTypes, string state);
    }
}