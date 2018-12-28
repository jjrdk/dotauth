namespace SimpleAuth.Uma.Tests.MiddleWares
{
    public class UserStore
    {
        private static UserStore _instance;
        private static readonly string _defaultClient = "client";

        private UserStore()
        {
            ClientId = _defaultClient;
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