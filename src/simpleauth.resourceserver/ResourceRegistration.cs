namespace SimpleAuth.ResourceServer
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the resource registration.
    /// </summary>
    [DataContract]
    public class ResourceRegistration
    {
        /// <summary>
        /// Gets or sets the id of the resource set.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the resource set id.
        /// </summary>
        [DataMember(Name = "resource_set_id")]
        public string ResourceSetId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource description.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        /// <value>The scopes.</value>
        [DataMember(Name = "resource_scopes")]
        public string[] Scopes { get; set; }

        /// <summary>
        /// Gets or sets the icon URI.
        /// </summary>
        /// <value>The icon URI.</value>
        [DataMember(Name = "icon_uri")]
        public Uri IconUri { get; set; }

        /// <summary>
        /// Gets or sets the resource owner.
        /// </summary>
        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the access policy <see cref="Uri"/>.
        /// </summary>
        [DataMember(Name = "access_policy")]
        public Uri AccessPolicy { get; set; }

        /// <summary>
        /// Gets or sets the resource mime type.
        /// </summary>
        [DataMember(Name = "mime_type")]
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the resource file name
        /// </summary>
        [DataMember(Name = "location")]
        public string Location { get; set; }
    }
}