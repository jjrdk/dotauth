namespace SimpleAuth.Twilio.Actions
{
    using System.Threading.Tasks;

    public interface IGenerateAndSendSmsCodeOperation
    {
        Task<string> Execute(string phoneNumber);
    }
}