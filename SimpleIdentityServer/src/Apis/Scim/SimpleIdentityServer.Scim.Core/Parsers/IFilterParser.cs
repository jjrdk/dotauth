namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public interface IFilterParser
    {
        Filter Parse(string path);
        string GetTarget(string path);
    }
}