namespace SimpleIdentityServer.Core.Logging
{
    public interface IOAuthEventSource : IEventSource
    {
        void StartAuthorization(
            string clientId,
            string responseType,
            string scope,
            string individualClaims);

        void StartAuthorizationCodeFlow(
            string clientId,
            string scope,
            string individualClaims);

        void StartProcessingAuthorizationRequest(
            string jsonAuthorizationRequest);

        void EndProcessingAuthorizationRequest(
            string jsonAuthorizationRequest,
            string actionType,
            string actionName);

        void StartGeneratingAuthorizationResponseToClient(
            string clientId,
            string responseTypes);

        void GrantAuthorizationCodeToClient(
            string clientId,
            string authorizationCode,
            string scopes);

        void EndGeneratingAuthorizationResponseToClient(
            string clientId,
            string parameters);

        void EndAuthorizationCodeFlow(
            string clientId,
            string actionType,
            string actionName);

        void StartImplicitFlow(
            string clientId,
            string scope,
            string individualClaims);

        void EndImplicitFlow(
            string clientId,
            string actionType,
            string actionName);

        void StartHybridFlow(
            string clientId,
            string scope,
            string individualClaims);

        void EndHybridFlow(
            string clientId,
            string actionType,
            string actionName);

        void EndAuthorization(
            string actionType,
            string controllerAction,
            string parameters);

        void GrantAccessToClient(string clientId, string accessToken, string scopes);

        void StartGetTokenByResourceOwnerCredentials(
            string clientId, 
            string userName,
            string password);

        void EndGetTokenByResourceOwnerCredentials(
            string accessToken,
            string identityToken);

        void StartGetTokenByAuthorizationCode(
            string clientId,
            string authorizationCode);

        void EndGetTokenByAuthorizationCode(
            string accessToken,
            string identityToken);

        void StartToAuthenticateTheClient(
            string clientId,
            string authenticationType);

        void FinishToAuthenticateTheClient(
            string clientId,
            string authenticateType);

        void StartGetTokenByRefreshToken(string refreshToken);

        void EndGetTokenByRefreshToken(
            string accessToken,
            string identityToken);

        void StartGetTokenByClientCredentials(
            string scope);

        void EndGetTokenByClientCredentials(
            string clientId,
            string scope);

        void StartRevokeToken(string token);

        void EndRevokeToken(string token);

        void StartRegistration(string clientName);
        void EndRegistration(string clientId, string clientName);

        void OAuthFailure(string code, 
            string description, 
            string state);
    }
}