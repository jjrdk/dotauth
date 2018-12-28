namespace SimpleAuth.Scim.Client.Tests.MiddleWares
{
    using System;

    public class ScimUserStore
    {
        private static ScimUserStore _instance;
        private static readonly string _defaultSubject = "administrator";

        private ScimUserStore()
        {
            Subject = _defaultSubject;
        }

        public static ScimUserStore Instance()
        {
            if (_instance == null)
            {
                _instance = new ScimUserStore();
            }

            return _instance;
        }

        public bool IsInactive { get; set; }
        public string Subject { get; set; }
        public string ScimId { get; set; }
        public DateTimeOffset? AuthenticationOffset { get; set; }
    }
}