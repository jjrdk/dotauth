namespace SimpleIdentityServer.Uma.Common.DTOs
{
    using System.Collections.Generic;
    using Extensions;

    public class PermissionResponse : Dictionary<string, object>
    {
        public string ResourceSetId
        {
            get => this.GetString(IntrospectPermissionNames.ResourceSetIdName);
            set => this.SetValue(IntrospectPermissionNames.ResourceSetIdName, value);
        }

        public IEnumerable<string> Scopes
        {
            get => this.GetObject<IEnumerable<string>>(IntrospectPermissionNames.ScopesName);
            set => this.SetObject(IntrospectPermissionNames.ScopesName, value);
        }

        public double Expiration
        {
            get => this.GetDouble(IntrospectPermissionNames.ExpirationName);
            set => this.SetValue(IntrospectPermissionNames.ExpirationName, value);
        }
    }
}