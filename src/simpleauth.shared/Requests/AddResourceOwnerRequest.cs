namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AddResourceOwnerRequest
    {
        [DataMember(Name = SharedConstants.AddResourceOwnerRequestNames.Subject)]
        public string Subject { get; set; }
        [DataMember(Name = SharedConstants.AddResourceOwnerRequestNames.Password)]
        public string Password { get; set; }
    }
}
