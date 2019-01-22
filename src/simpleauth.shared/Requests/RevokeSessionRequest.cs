namespace SimpleAuth.Shared.Requests
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class RevokeSessionRequest
    {
        [DataMember(Name = RevokeSessionRequestNames.IdTokenHint)]
        public string IdTokenHint { get; set; }
        [DataMember(Name = RevokeSessionRequestNames.PostLogoutRedirectUri)]
        public Uri PostLogoutRedirectUri { get; set; }
        [DataMember(Name = RevokeSessionRequestNames.State)]
        public string State { get; set; }
    }
}
