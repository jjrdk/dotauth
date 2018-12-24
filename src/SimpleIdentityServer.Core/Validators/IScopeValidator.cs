namespace SimpleIdentityServer.Core.Validators
{
    using SimpleAuth.Shared.Models;

    public interface IScopeValidator
    {
        ScopeValidationResult Check(string scope, Client client);
    }
}