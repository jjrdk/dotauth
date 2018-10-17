namespace SimpleIdentityServer.Host.Tests.MiddleWares
{
    using System;

    public class UserStore
    {
        private static UserStore _instance;
        private static string _defaultSubject = "administrator";

        private UserStore()
        {
            Subject = _defaultSubject;
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
        public string Subject { get; set; }
        public DateTimeOffset? AuthenticationOffset { get; set; }
    }
}