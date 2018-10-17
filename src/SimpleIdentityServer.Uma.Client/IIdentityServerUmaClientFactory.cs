namespace SimpleIdentityServer.Uma.Client
{
    using Permission;
    using Policy;
    using ResourceSet;

    public interface IIdentityServerUmaClientFactory
    {
        IPermissionClient GetPermissionClient();
        IResourceSetClient GetResourceSetClient();
        IPolicyClient GetPolicyClient();
    }
}