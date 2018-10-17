namespace SimpleIdentityServer.Scim.Core.EF.Helpers
{
    using SimpleIdentityServer.Core.Common.DTOs;
    using SimpleIdentityServer.Core.Common.Models;

    public interface ITransformers
    {
        SchemaAttributeResponse Transform(Models.SchemaAttribute attr);
        RepresentationAttribute Transform(Models.RepresentationAttribute attr);
        Models.RepresentationAttribute Transform(RepresentationAttribute attr);
    }
}