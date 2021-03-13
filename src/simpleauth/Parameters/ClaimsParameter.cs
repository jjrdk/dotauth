namespace SimpleAuth.Parameters
{
    using System;

    internal record ClaimsParameter
    {
        public ClaimParameter[] UserInfo { get; init; } = Array.Empty<ClaimParameter>();

        public ClaimParameter[] IdToken { get; init; } = Array.Empty<ClaimParameter>();
    }
}
