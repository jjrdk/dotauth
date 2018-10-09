using SimpleIdentityServer.Scim.Mapping.Ad.Models;

namespace SimpleIdentityServer.Scim.Mapping.Ad.Stores
{
    public interface IConfigurationStore
    {
        bool UpdateConfiguration(AdConfiguration adConfiguration);
        AdConfiguration GetConfiguration();
    }
}
