namespace SimpleAuth.Twilio
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the Twilio client interface.
    /// </summary>
    public interface ITwilioClient
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        /// <param name="toPhoneNumber">To phone number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        Task<bool> SendMessage(TwilioSmsCredentials credentials, string toPhoneNumber, string message);
    }
}