namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    [DataContract]
    public class OrderRequest
    {
        [DataMember(Name = SharedConstants.OrderRequestNames.Target)]
        public string Target { get; set; }

        [DataMember(Name = SharedConstants.OrderRequestNames.Type)]
        public int Type { get; set; }
    }
}
