namespace SimpleAuth.Twilio
{
    using System.Threading.Tasks;

    public interface ITwilioClient
    {
        Task<bool> SendMessage(TwilioSmsCredentials credentials, string toPhoneNumber, string message);
    }
}