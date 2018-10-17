namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using SimpleIdentityServer.Core.Common.Models;

    public class ParseRepresentationResult
    {
        public Representation Representation { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsParsed { get; set; }
    }
}