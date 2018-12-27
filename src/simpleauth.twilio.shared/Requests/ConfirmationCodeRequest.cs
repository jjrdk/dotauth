namespace SimpleAuth.Twilio.Shared.Requests
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ConfirmationCodeRequest
    {
        [DataMember(Name = Constants.ConfirmationCodeRequestNames.PhoneNumber)]
        public string PhoneNumber { get; set; }
    }
}