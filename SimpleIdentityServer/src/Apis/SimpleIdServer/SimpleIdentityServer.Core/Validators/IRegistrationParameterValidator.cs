namespace SimpleIdentityServer.Core.Validators
{
    using Parameters;

    public interface IRegistrationParameterValidator
    {
        void Validate(RegistrationParameter parameter);
    }
}