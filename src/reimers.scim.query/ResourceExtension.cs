namespace Reimers.Scim.Query
{
    public abstract class ResourceExtension
    {
        [ScimInternal]
        protected internal abstract string SchemaIdentifier { get; }

        public abstract int CalculateVersion();
    }
}