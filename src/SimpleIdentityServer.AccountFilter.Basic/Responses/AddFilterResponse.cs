namespace SimpleIdentityServer.AccountFilter.Basic.Responses
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AddFilterResponse
    {
        [DataMember(Name = Constants.FilterResponseNames.Id)]
        public string Id { get; set; }
    }
}
