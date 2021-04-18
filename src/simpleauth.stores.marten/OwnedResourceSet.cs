namespace SimpleAuth.Stores.Marten
{
    using System.Runtime.Serialization;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the owned resource set.
    /// </summary>
    public record OwnedResourceSet : ResourceSet
    {
        /// <summary>
        /// Gets or sets the resource set owner.
        /// </summary>
        [DataMember(Name = "owner")]
        public string Owner { get; init; } = null!;

        /// <summary>
        /// Returns the resource set base.
        /// </summary>
        /// <returns></returns>
        public ResourceSet AsResourceSet()
        {
            return new()
            {
                AuthorizationPolicies = AuthorizationPolicies,
                Description = Description,
                IconUri = IconUri,
                Id = Id,
                Name = Name,
                Scopes = Scopes,
                Type = Type
            };
        }

        /// <summary>
        /// Create an <see cref="OwnedResourceSet"/> instance from a <see cref="ResourceSet"/> instance.
        /// </summary>
        /// <param name="resourceSet">The base resource set.</param>
        /// <param name="owner">The resource set owner.</param>
        /// <returns></returns>
        public static OwnedResourceSet FromResourceSet(ResourceSet resourceSet, string owner)
        {
            return new()
            {
                AuthorizationPolicies = resourceSet.AuthorizationPolicies,
                Description = resourceSet.Description,
                IconUri = resourceSet.IconUri,
                Id = resourceSet.Id,
                Name = resourceSet.Name,
                Owner = owner,
                Scopes = resourceSet.Scopes,
                Type = resourceSet.Type
            };
        }
    }
}