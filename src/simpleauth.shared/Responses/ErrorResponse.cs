namespace SimpleIdentityServer.Shared.Responses
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ErrorResponse
    {
        [DataMember(Name = SharedConstants.ErrorResponseNames.Error)]
        public string Error { get; set; }
        [DataMember(Name = SharedConstants.ErrorResponseNames.ErrorDescription)]
        public string ErrorDescription { get; set; }
        [DataMember(Name = SharedConstants.ErrorResponseNames.ErrorUri)]
        public string ErrorUri { get; set; }
    }
}
