namespace SimpleIdentityServer.Core.Validators
{
    using Parameters;

    public interface IRevokeTokenParameterValidator
    {
        void Validate(RevokeTokenParameter parameter);
    }
}