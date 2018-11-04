namespace SimpleIdentityServer.Logging
{
    public interface IOpenIdEventSource : IEventSource
    {
        void GiveConsent(string subject, string clientId, string consentId);
        void AuthenticateResourceOwner(string subject);
        void GetConfirmationCode(string code);
        void InvalidateConfirmationCode(string code);
        void ConfirmationCodeNotValid(string code);

        void AddResourceOwner(string subject);

        void OpenIdFailure(string code,
            string description,
            string state);
    }
}