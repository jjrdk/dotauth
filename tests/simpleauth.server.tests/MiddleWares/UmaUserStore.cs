namespace SimpleAuth.Server.Tests.MiddleWares
{
    public class UmaUserStore
    {
        private static UmaUserStore _instance;
        private static readonly string DefaultClient = "client";

        private UmaUserStore()
        {
            ClientId = DefaultClient;
        }

        public static UmaUserStore Instance()
        {
            return _instance ??= new UmaUserStore();
        }

        public bool IsInactive { get; set; }
        public string ClientId { get; set; }
    }
}