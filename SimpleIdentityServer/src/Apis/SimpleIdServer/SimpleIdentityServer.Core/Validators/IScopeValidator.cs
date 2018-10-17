namespace SimpleIdentityServer.Core.Validators
{
    public interface IScopeValidator
    {
        ScopeValidationResult Check(string scope, Common.Models.Client client);
    }
}