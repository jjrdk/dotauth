namespace SimpleAuth.Shared
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ConfirmationCodeRequest
    {
        [DataMember(Name = "phone_number")]
        public string PhoneNumber { get; set; }
    }
}