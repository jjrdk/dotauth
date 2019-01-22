namespace SimpleAuth.Uma.Tests.MiddleWares
{
    public class UserStore
    {
        private static UserStore _instance;
        private static readonly string DefaultClient = "client";

        private UserStore()
        {
            ClientId = DefaultClient;
        }

        public static UserStore Instance()
        {
            if (_instance == null)
            {
                _instance = new UserStore();
            }

            return _instance;
        }

        public bool IsInactive { get; set; }
        public string ClientId { get; set; }
    }
}