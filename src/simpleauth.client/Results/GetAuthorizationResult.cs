namespace SimpleAuth.Client.Results
{
    using System;

    public class GetAuthorizationResult : BaseSidResult
    {
        public Uri Location { get; set; }
    }
}
