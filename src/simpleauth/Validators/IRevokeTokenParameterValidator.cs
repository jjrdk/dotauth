namespace SimpleAuth.Validators
{
    using Parameters;

    public interface IRevokeTokenParameterValidator
    {
        void Validate(RevokeTokenParameter parameter);
    }
}