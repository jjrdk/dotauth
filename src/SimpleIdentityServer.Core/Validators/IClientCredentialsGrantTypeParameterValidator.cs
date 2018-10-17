namespace SimpleIdentityServer.Core.Validators
{
    using Parameters;

    public interface IClientCredentialsGrantTypeParameterValidator
    {
        void Validate(ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter);
    }
}