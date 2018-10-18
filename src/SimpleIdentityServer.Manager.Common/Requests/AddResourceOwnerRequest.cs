using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    [DataContract]
    public class AddResourceOwnerRequest
    {
        [DataMember(Name = Constants.AddResourceOwnerRequestNames.Subject)]
        public string Subject { get; set; }
        [DataMember(Name = Constants.AddResourceOwnerRequestNames.Password)]
        public string Password { get; set; }
    }
}
