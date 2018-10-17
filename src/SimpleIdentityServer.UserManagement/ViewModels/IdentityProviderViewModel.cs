namespace SimpleIdentityServer.UserManagement.ViewModels
{
    public class IdentityProviderViewModel
    {
        public IdentityProviderViewModel(string displayName)
        {
            DisplayName = displayName;
        }

        public IdentityProviderViewModel(string displayName, string externalSubject) : this(displayName)
        {
            ExternalSubject = externalSubject;
        }

        public string DisplayName { get; set; }
        public string ExternalSubject { get; set; }
    }
}