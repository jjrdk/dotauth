namespace SimpleAuth.Twilio.Actions
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    public interface ISmsAuthenticationOperation
    {
        Task<ResourceOwner> Execute(string phoneNumber);
    }
}