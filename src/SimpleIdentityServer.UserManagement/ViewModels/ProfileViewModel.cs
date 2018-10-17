using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement.ViewModels
{
    public class ProfileViewModel
    {
        public ProfileViewModel()
        {
            LinkedIdentityProviders = new List<IdentityProviderViewModel>();
            UnlinkedIdentityProviders = new List<IdentityProviderViewModel>();
        }

        public ICollection<IdentityProviderViewModel> LinkedIdentityProviders { get; set; }
        public ICollection<IdentityProviderViewModel> UnlinkedIdentityProviders { get; set; }
    }
}
