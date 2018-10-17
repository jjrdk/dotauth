namespace SimpleIdentityServer.Core.Validators
{
    using Parameters;

    public interface IIntrospectionParameterValidator
    {
        void Validate(IntrospectionParameter introspectionParameter);
    }
}