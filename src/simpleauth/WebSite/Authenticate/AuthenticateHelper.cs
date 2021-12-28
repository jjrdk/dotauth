namespace SimpleAuth.WebSite.Authenticate
{
    using SimpleAuth.Api.Authorization;
    using SimpleAuth.Common;
    using SimpleAuth.Parameters;
    using SimpleAuth.Results;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Events;
    using SimpleAuth.Extensions;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Properties;

    internal sealed class AuthenticateHelper
    {
        private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly IConsentRepository _consentRepository;
        private readonly IClientStore _clientRepository;
        private readonly ILogger _logger;

        public AuthenticateHelper(
            IAuthorizationCodeStore authorizationCodeStore,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IConsentRepository consentRepository,
            IClientStore clientRepository,
            IJwksStore jwksStore,
            IEventPublisher eventPublisher,
            ILogger logger)
        {
            _generateAuthorizationResponse = new GenerateAuthorizationResponse(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                clientRepository,
                consentRepository,
                jwksStore,
                eventPublisher,
                logger);
            _consentRepository = consentRepository;
            _clientRepository = clientRepository;
            _logger = logger;
        }

        public async Task<EndpointResult> ProcessRedirection(
            AuthorizationParameter authorizationParameter,
            string? code,
            string subject,
            Claim[] claims,
            string? issuerName,
            CancellationToken cancellationToken)
        {
            var client = authorizationParameter.ClientId == null
                ? null
                : await _clientRepository.GetById(authorizationParameter.ClientId, cancellationToken)
                    .ConfigureAwait(false);
            if (client == null)
            {
                throw new InvalidOperationException(SharedStrings.TheClientDoesntExist);
            }

            // Redirect to the consent page if the prompt parameter contains "consent"
            EndpointResult result;
            var prompts = authorizationParameter.Prompt.ParsePrompts();
            if (prompts.Contains(PromptParameters.Consent) && code != null)
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
                if (responseMode != ResponseModes.None)
                {
                    return result with
                    {
                        RedirectInstruction = result.RedirectInstruction! with { ResponseMode = responseMode }
                    };
                }

                var responseTypes = authorizationParameter.ResponseType.ParseResponseTypes();
                var authorizationFlow = GetAuthorizationFlow(authorizationParameter.State, responseTypes);
                switch (authorizationFlow)
                {
                    case Option<AuthorizationFlow>.Error e:
                        return EndpointResult.CreateBadRequestResult(e.Details);
                    case Option<AuthorizationFlow>.Result r:
                        responseMode = GetResponseMode(r.Item);
                        break;
                }

                return result with
                {
                    RedirectInstruction = result.RedirectInstruction! with { ResponseMode = responseMode }
                };
            }

            // If there's no consent & there's no consent prompt then redirect to the consent screen.
            return EndpointResult.CreateAnEmptyActionResultWithRedirection(
                SimpleAuthEndPoints.ConsentIndex,
                new Parameter("code", code ?? ""));
        }

        private Option<AuthorizationFlow> GetAuthorizationFlow(string? state, params string[] responseTypes)
        {
            var record = CoreConstants.MappingResponseTypesToAuthorizationFlows.Keys
                .SingleOrDefault(k => k.Length == responseTypes.Length && k.All(responseTypes.Contains));
            if (record != null)
            {
                return new Option<AuthorizationFlow>.Result(
                    CoreConstants.MappingResponseTypesToAuthorizationFlows[record]);
            }

            _logger.LogError(Strings.TheAuthorizationFlowIsNotSupported);
            return new Option<AuthorizationFlow>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InvalidRequest,
                    Detail = Strings.TheAuthorizationFlowIsNotSupported,
                    Status = HttpStatusCode.BadRequest
                },
                state);

        }

        private static string GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return CoreConstants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }
    }
}
