namespace SimpleIdentityServer.Uma.Core.Validators
{
    using Models;

    public interface IResourceSetParameterValidator
    {
        void CheckResourceSetParameter(ResourceSet resourceSet);
    }
}