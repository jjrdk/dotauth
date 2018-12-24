namespace SimpleIdentityServer.Shared
{
    using System.Runtime.Serialization;

    public enum SortOrders
    {
        [EnumMember(Value = ScimConstants.SortOrderNames.Ascending)]
        Ascending,
        [EnumMember(Value = ScimConstants.SortOrderNames.Descending)]
        Descending
    }
}