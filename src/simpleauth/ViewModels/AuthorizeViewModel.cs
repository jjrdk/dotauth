namespace SimpleAuth.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class AuthorizeViewModel : IdProviderAuthorizeViewModel
    {
        [Required(ErrorMessage = "the user name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "the password is required")]
        public string Password { get; set; }
        public bool IsChecked { get; set; }
    }
}