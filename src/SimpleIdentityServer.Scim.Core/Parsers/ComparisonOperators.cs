namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public enum ComparisonOperators
    {
        // Equal
        eq,
        // Not equal
        ne,
        // Contains
        co,
        // Starts with
        sw,
        // Ends with
        ew,
        // Present (has value)
        pr,
        // Greater than
        gt,
        // Greater than or equal to
        ge,
        // Less than
        lt,
        // Less than or equal to
        le
    }
}