namespace SimpleIdentityServer.Logging
{
    public interface IUmaServerEventSource : IEventSource
    {
        void StartGettingAuthorization(string request);

        void CheckAuthorizationPolicy(string request);

        void RequestIsNotAuthorized(string request);

        void RequestIsAuthorized(string request);

        void AuthorizationPoliciesFailed(string ticketId);

        void StartToIntrospect(string rpt);

        void RptHasExpired(string rpt);

        void EndIntrospection(string result);

        void StartAddPermission(string request);
        void FinishAddPermission(string request);

        void StartAddingAuthorizationPolicy(string request);
        void FinishToAddAuthorizationPolicy(string result);
        void StartToRemoveAuthorizationPolicy(string policyId);
        void FinishToRemoveAuthorizationPolicy(string policyId);
        void StartAddResourceToAuthorizationPolicy(string policy, string resourceId);
        void FinishAddResourceToAuthorizationPolicy(string policy, string resourceId);
        void StartRemoveResourceFromAuthorizationPolicy(string policy, string resourceId);
        void FinishRemoveResourceFromAuthorizationPolicy(string policy, string resourceId);
        void StartUpdateAuthorizationPolicy(string request);
        void FinishUpdateAuhthorizationPolicy(string request);

        void StartToAddResourceSet(string request);
        void FinishToAddResourceSet(string result);
        void StartToRemoveResourceSet(string resourceSetId);
        void FinishToRemoveResourceSet(string resourceSetId);
        void StartToUpdateResourceSet(string request);
        void FinishToUpdateResourceSet(string request);

        void StartToAddScope(string request);

        void FinishToAddScope(string result);

        void StartToRemoveScope(string scope);

        void FinishToRemoveScope(string scope);
    }
}