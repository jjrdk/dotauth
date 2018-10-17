namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using System;

    [Flags]
    public enum LogicalOperators
    {
        // Logical and
        and = 1,
        // Logical or
        or = 2,
        // Not function
        not = 4
    }
}