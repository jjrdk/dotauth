namespace SimpleAuth.WebSite.Authenticate
{
    using SimpleAuth.Api.Authorization;
    using SimpleAuth.Common;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Parameters;
    using SimpleAuth.Results;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Events;
    using SimpleAuth.Extensions;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared.Errors;

    internal sealed class AuthenticateHelper
    {
        private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly IConsentRepository _consentRepository;
        private readonly IClientStore _clientRepository;

        public AuthenticateHelper(
            IAuthorizationCodeStore authorizationCodeStore,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IConsentRepository consentRepository,
            IClientStore clientRepository,
            IJwksStore jwksStore,
            IEventPublisher eventPublisher)
        {
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                clientRepository,
                consentRepository,
                jwksStore,
                eventPublisher);
            _consentRepository = consentRepository;
            _clientRepository = clientRepository;
        }

        public async Task<EndpointResult> ProcessRedirection(
            AuthorizationParameter authorizationParameter,
            string code,
            string subject,
            Claim[] claims,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var client = authorizationParameter.ClientId == null
                ? null
                : await _clientRepository.GetById(authorizationParameter.ClientId, cancellationToken)
                    .ConfigureAwait(false);
            if (client == null)
            {
                throw new InvalidOperationException(Strings.TheClientDoesntExist);
            }

            // Redirect to the consent page if the prompt parameter contains "consent"
            EndpointResult result;
            var prompts = authorizationParameter.Prompt.ParsePrompts();
            if (prompts.Contains(PromptParameters.Consent))
            {
                return EndpointResult.CreateAnEmptyActionResultWithRedirection(
                    SimpleAuthEndPoints.ConsentIndex,
                    new Parameter("code", code));
            }

            var assignedConsent = await _consentRepository
                .GetConfirmedConsents(subject, authorizationParameter, cancellationToken)
                .ConfigureAwait(false);

            // If there's already one consent then redirect to the callback
            if (assignedConsent != null)
            {
                var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                result = await _generateAuthorizationResponse.Generate(
                        EndpointResult.CreateAnEmptyActionResultWithRedirectionToCallBackUrl(),
                        authorizationParameter,
                        claimsPrincipal,
                        client,
                        issuerName,
                        cancellationToken)
                    .ConfigureAwait(false);
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseModes.None)
                {
                    var responseTypes = authorizationParameter.ResponseType.ParseResponseTypes();
                    var authorizationFlow = GetAuthorizationFlow(authorizationParameter.State, responseTypes);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                return result with
                {
                    RedirectInstruction = result.RedirectInstruction! with {ResponseMode = responseMode}
                };
            }

            // If there's no consent & there's no consent prompt then redirect to the consent screen.
            return EndpointResult.CreateAnEmptyActionResultWithRedirection(
                SimpleAuthEndPoints.ConsentIndex,
                new Parameter("code", code));
        }

        private static AuthorizationFlow GetAuthorizationFlow(string? state, params string[] responseTypes)
        {
            if (responseTypes == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    Strings.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            var record = CoreConstants.MappingResponseTypesToAuthorizationFlows.Keys
                .SingleOrDefault(k => k.Length == responseTypes.Length && k.All(responseTypes.Contains));
            if (record == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    Strings.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            return CoreConstants.MappingResponseTypesToAuthorizationFlows[record];
        }

        private static string GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return CoreConstants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }
    }
}
