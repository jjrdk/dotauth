namespace SimpleAuth.Validators
{
    using Parameters;

    public interface IUpdateResourceOwnerPasswordParameterValidator
    {
        void Validate(UpdateResourceOwnerPasswordParameter parameter);
    }
}