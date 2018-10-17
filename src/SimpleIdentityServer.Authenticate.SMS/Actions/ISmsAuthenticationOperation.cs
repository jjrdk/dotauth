namespace SimpleIdentityServer.Authenticate.SMS.Actions
{
    using System.Threading.Tasks;
    using Core.Common.Models;

    public interface ISmsAuthenticationOperation
    {
        Task<ResourceOwner> Execute(string phoneNumber);
    }
}