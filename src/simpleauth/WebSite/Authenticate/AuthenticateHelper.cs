namespace SimpleAuth.WebSite.Authenticate
{
    using SimpleAuth.Api.Authorization;
    using SimpleAuth.Common;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Parameters;
    using SimpleAuth.Results;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Extensions;
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
            var client = await _clientRepository.GetById(authorizationParameter.ClientId, cancellationToken).ConfigureAwait(false);
            if (client == null)
            {
                throw new InvalidOperationException(ErrorMessages.TheClientDoesntExist);
            }

            // Redirect to the consent page if the prompt parameter contains "consent"
            EndpointResult result;
            var prompts = authorizationParameter.Prompt.ParsePrompts();
            if (prompts != null &&
                prompts.Contains(PromptParameters.Consent))
            {
                result = EndpointResult.CreateAnEmptyActionResultWithRedirection();
                result.RedirectInstruction.Action = SimpleAuthEndPoints.ConsentIndex;
                result.RedirectInstruction.AddParameter("code", code);
                return result;
            }

            var assignedConsent = await _consentRepository.GetConfirmedConsents(subject, authorizationParameter, cancellationToken)
                .ConfigureAwait(false);

            // If there's already one consent then redirect to the callback
            if (assignedConsent != null)
            {
                result = EndpointResult.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await _generateAuthorizationResponse
                    .Generate(result, authorizationParameter, claimsPrincipal, client, issuerName, cancellationToken)
                    .ConfigureAwait(false);
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseModes.None)
                {
                    var responseTypes = authorizationParameter.ResponseType.ParseResponseTypes();
                    var authorizationFlow = GetAuthorizationFlow(authorizationParameter.State, responseTypes);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                result.RedirectInstruction.ResponseMode = responseMode;
                return result;
            }

            // If there's no consent & there's no consent prompt then redirect to the consent screen.
            result = EndpointResult.CreateAnEmptyActionResultWithRedirection();
            result.RedirectInstruction.Action = SimpleAuthEndPoints.ConsentIndex;
            result.RedirectInstruction.AddParameter("code", code);
            return result;
        }

        private static AuthorizationFlow GetAuthorizationFlow(string state, params string[] responseTypes)
        {
            if (responseTypes == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    ErrorMessages.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            var record = CoreConstants.MappingResponseTypesToAuthorizationFlows.Keys
                .SingleOrDefault(k => k.Length == responseTypes.Length && k.All(responseTypes.Contains));
            if (record == null)
            {
                throw new SimpleAuthExceptionWithState(
                    ErrorCodes.InvalidRequest,
                    ErrorMessages.TheAuthorizationFlowIsNotSupported,
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
