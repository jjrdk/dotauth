namespace SimpleIdentityServer.Scim.Core.Validators
{
    public interface IParametersValidator
    {
        void ValidateLocationPattern(string locationPattern);
    }
}