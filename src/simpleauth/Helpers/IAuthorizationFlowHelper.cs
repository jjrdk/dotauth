namespace SimpleAuth.Helpers
{
    using System.Collections.Generic;
    using Api.Authorization;

    public interface IAuthorizationFlowHelper
    {
        AuthorizationFlow GetAuthorizationFlow(ICollection<string> responseTypes, string state);
    }
}