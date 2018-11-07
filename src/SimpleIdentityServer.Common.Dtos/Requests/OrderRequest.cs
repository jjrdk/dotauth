namespace SimpleIdentityServer.Shared.Requests
{
    using System.Runtime.Serialization;
    using Shared;

    [DataContract]
    public class OrderRequest
    {
        [DataMember(Name = Constants.OrderRequestNames.Target)]
        public string Target { get; set; }

        [DataMember(Name = Constants.OrderRequestNames.Type)]
        public int Type { get; set; }
    }
}
