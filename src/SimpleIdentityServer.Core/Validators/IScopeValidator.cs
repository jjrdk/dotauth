namespace SimpleIdentityServer.Core.Validators
{
    using Shared.Models;

    public interface IScopeValidator
    {
        ScopeValidationResult Check(string scope, Client client);
    }
}