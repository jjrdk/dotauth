namespace SimpleAuth.Sms;

using System.Threading.Tasks;

/// <summary>
/// Defines the SMS client interface.
/// </summary>
public interface ISmsClient
{
    /// <summary>
    /// Sends the message.
    /// </summary>
    /// <param name="toPhoneNumber">To phone number.</param>
    /// <param name="message">The message.</param>
    /// <returns></returns>
    Task<(bool,string?)> SendMessage(string toPhoneNumber, string message);
}