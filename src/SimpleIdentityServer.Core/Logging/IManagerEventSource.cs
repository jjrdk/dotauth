namespace SimpleIdentityServer.Core.Logging
{
    public interface IManagerEventSource : IEventSource
    {
        void StartToRemoveClient(string clientId);
        void FinishToRemoveClient(string clientId);
        void StartToUpdateClient(string request);
        void FinishToUpdateClient(string request);

        void StartToRemoveResourceOwner(string subject);
        void FinishToRemoveResourceOwner(string subject);
        void StartToUpdateResourceOwnerClaims(string subject);
        void FinishToUpdateResourceOwnerClaims(string subject);
        void StartToUpdateResourceOwnerPassword(string subject);
        void FinishToUpdateResourceOwnerPassword(string subject);
        void StartToAddResourceOwner(string subject);
        void FinishToAddResourceOwner(string subject);

        void StartToRemoveScope(string scope);

        void FinishToRemoveScope(string scope);

        void StartToExport();

        void FinishToExport();

        void StartToImport();

        void RemoveAllClients();

        void FinishToImport();
    }
}