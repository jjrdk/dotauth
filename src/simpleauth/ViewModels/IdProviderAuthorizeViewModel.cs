namespace SimpleAuth.ViewModels
{
    using System.Collections.Generic;

    public abstract class IdProviderAuthorizeViewModel
    {
        protected IdProviderAuthorizeViewModel()
        {
            IdProviders = new List<IdProviderViewModel>();
        }

        public List<IdProviderViewModel> IdProviders { get; set; }
    }
}