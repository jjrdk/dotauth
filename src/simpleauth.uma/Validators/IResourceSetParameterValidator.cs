namespace SimpleAuth.Uma.Validators
{
    using Models;

    public interface IResourceSetParameterValidator
    {
        void CheckResourceSetParameter(ResourceSet resourceSet);
    }
}