namespace SimpleIdentityServer.Host
{
    public static class HostEnpoints
    {
        public const string RootPath = "/api";
        public const string Jws = RootPath + "/jws";
        public const string Jwe = RootPath + "/jwe";
        public const string Clients = RootPath + "/clients";
        public const string Scopes = RootPath + "/scopes";
        public const string ResourceOwners = RootPath + "/resource_owners";
        public const string Manage = RootPath + "/manage";
        public const string Claims = RootPath + "/claims";
        //public const string Configuration = ".well-known/openid-configuration";
        public const string Configuration = ".well-known/openidmanager-configuration";
    }
}
