namespace SimpleAuth.Validators
{
    using Parameters;

    public interface IUpdateResourceOwnerClaimsParameterValidator
    {
        void Validate(UpdateResourceOwnerClaimsParameter parameter);
    }
}