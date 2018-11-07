using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Core.Parameters
{
    public class UpdateResourceOwnerClaimsParameter
    {
        public string Login { get; set; }
        public List<KeyValuePair<string, string>> Claims { get; set; }
    }
}
