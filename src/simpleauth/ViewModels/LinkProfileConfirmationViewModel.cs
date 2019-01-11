namespace SimpleAuth.Server.ViewModels
{
    public class LinkProfileConfirmationViewModel
    {
        public LinkProfileConfirmationViewModel(string issuer)
        {
            Issuer = issuer;
        }

        public string Issuer { get; set; }
    }
}
