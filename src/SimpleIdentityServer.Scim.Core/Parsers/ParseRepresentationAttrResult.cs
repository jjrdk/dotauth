namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using SimpleIdentityServer.Core.Common.Models;

    public class ParseRepresentationAttrResult
    {
        public RepresentationAttribute RepresentationAttribute { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsParsed { get; set; }
    }
}