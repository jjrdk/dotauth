namespace SimpleAuth.Twilio.Actions
{
    using System.Threading.Tasks;
    using Shared.Models;

    public interface ISmsAuthenticationOperation
    {
        Task<ResourceOwner> Execute(string phoneNumber);
    }
}