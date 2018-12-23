namespace SimpleIdentityServer.Core.Validators
{
    using Parameters;

    public interface IUpdateResourceOwnerClaimsParameterValidator
    {
        void Validate(UpdateResourceOwnerClaimsParameter parameter);
    }
}