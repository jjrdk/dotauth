namespace SimpleIdentityServer.Core.Parameters
{
    using System;

    [Flags]
    public enum PromptParameter
    {
        none,
        login,
        consent,
        select_account
    }
}