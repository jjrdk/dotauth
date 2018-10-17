namespace SimpleIdentityServer.Authenticate.SMS.Actions
{
    using System.Threading.Tasks;

    public interface IGenerateAndSendSmsCodeOperation
    {
        Task<string> Execute(string phoneNumber);
    }
}