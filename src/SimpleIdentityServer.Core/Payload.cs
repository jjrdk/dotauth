namespace SimpleIdentityServer.Core
{
    public class Payload
    {
        public string ClientId { get; set; }
        public object Authorization { get; set; }
        public object Content { get; set; }
    }
}