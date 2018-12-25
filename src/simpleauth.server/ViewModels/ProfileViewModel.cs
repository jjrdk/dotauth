namespace SimpleAuth.Server.ViewModels
{
    using System.Collections.Generic;

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
