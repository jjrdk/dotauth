using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimpleIdentityServer.UserManagement.ViewModels
{
    public class AuthorizeViewModel
    {
        public AuthorizeViewModel()
        {
            IdProviders = new List<IdProviderViewModel>();
        }

        [Required(ErrorMessage = "the user name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "the password is required")]
        public string Password { get; set; }
        public bool IsChecked { get; set; }
        public List<IdProviderViewModel> IdProviders { get; set; }
    }
}