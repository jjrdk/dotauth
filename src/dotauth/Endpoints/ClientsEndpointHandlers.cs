namespace DotAuth.Endpoints;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Properties;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using DotAuth.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class ClientsEndpointHandlers
{
    private const string CreateClientView = "/Views/Clients/Create.cshtml";
    private const string GetAllClientsView = "/Views/Clients/GetAll.cshtml";
    private const string GetClientView = "/Views/Clients/Get.cshtml";

    internal static async Task<IResult> Register(
        HttpContext httpContext,
        DynamicClientRegistrationRequest request,
        IRequestThrottle requestThrottle,
        IClientRepository clientRepository,
        IScopeStore scopeStore,
        [FromServices] IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (string.IsNullOrWhiteSpace(request.ClientName) || request.RedirectUris.Length == 0)
        {
            return Results.BadRequest();
        }

        var email = httpContext.User.GetEmail();
        if (email != null && !request.Contacts.Contains(email))
        {
            request.Contacts = request.Contacts.Append(email).ToArray();
        }

        Uri.TryCreate(request.LogoUri, UriKind.Absolute, out var logoUri);

        var client = new Client
        {
            ClientName = request.ClientName,
            LogoUri = logoUri,
            ApplicationType = request.ApplicationType == ApplicationTypes.Native ? ApplicationTypes.Native : ApplicationTypes.Web,
            RedirectionUrls = request.RedirectUris.Select(x => new Uri(x)).ToArray(),
            GrantTypes = [GrantTypes.AuthorizationCode, GrantTypes.ClientCredentials, GrantTypes.RefreshToken],
            ClientId = Id.Create(),
            Contacts = request.Contacts,
            ResponseTypes = ResponseTypeNames.All,
            RequirePkce = true,
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = Id.Create() }],
            TokenLifetime = TimeSpan.FromHours(1),
            UserClaimsToIncludeInAuthToken = [new Regex("^sub$", RegexOptions.Compiled)],
            TokenEndPointAuthMethod = request.TokenEndpointAuthMethod ?? TokenEndPointAuthenticationMethods.ClientSecretPost
        };

        var logger = CreateLogger(loggerFactory);
        var factory = new ClientFactory(httpClientFactory, scopeStore, DeserializeUris, logger);
        var toInsert = await factory.Build(client, cancellationToken: cancellationToken).ConfigureAwait(false);
        switch (toInsert)
        {
            case Option<Client>.Error e:
                return Results.Json(
                    new { error = e.Details.Title, error_description = e.Details.Detail },
                    statusCode: StatusCodes.Status500InternalServerError);
            case Option<Client>.Result r:
                var item = r.Item;
                var result = await clientRepository.Insert(item, cancellationToken).ConfigureAwait(false);
                var editUri = BuildAbsolutePath(httpContext, $"/{CoreConstants.EndPoints.Clients}/register/{item.ClientId}");
                return result
                    ? Results.Created(
                        editUri,
                        new DynamicClientRegistrationResponse
                        {
                            ApplicationType = item.ApplicationType,
                            ClientId = item.ClientId,
                            ClientName = item.ClientName,
                            ClientSecret = item.Secrets[0].Value,
                            RedirectUris = Array.ConvertAll(item.RedirectionUrls, u => u.AbsoluteUri),
                            ClientSecretExpiresAt = 0,
                            Contacts = item.Contacts,
                            TokenEndpointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                            RegistrationClientUri = editUri
                        })
                    : Results.BadRequest();
            default:
                throw new InvalidOperationException();
        }
    }

    internal static async Task<IResult> Modify(
        HttpContext httpContext,
        string clientId,
        DynamicClientRegistrationRequest update,
        IRequestThrottle requestThrottle,
        IClientRepository clientRepository,
        IScopeStore scopeStore,
        [FromServices] IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var logger = CreateLogger(loggerFactory);
        var client = await clientRepository.GetById(clientId, cancellationToken).ConfigureAwait(false);
        if (client == null)
        {
            logger.LogError("Client with id: {ClientId} not found", clientId);
            return Results.BadRequest();
        }

        if (update.RedirectUris.Length > 0)
        {
            client.RedirectionUrls = Array.ConvertAll(update.RedirectUris, x => new Uri(x));
        }

        client.LogoUri = Uri.TryCreate(update.LogoUri, UriKind.Absolute, out var logoUri) ? logoUri : null;
        if (update.Contacts.Length > 0)
        {
            client.Contacts = update.Contacts;
        }

        if (!string.IsNullOrWhiteSpace(update.ClientName))
        {
            client.ClientName = update.ClientName;
        }

        client.ApplicationType = update.ApplicationType == ApplicationTypes.Native ? ApplicationTypes.Native : ApplicationTypes.Web;
        if (!string.IsNullOrWhiteSpace(update.TokenEndpointAuthMethod))
        {
            client.TokenEndPointAuthMethod = update.TokenEndpointAuthMethod;
        }

        return await PutInternal(client, clientRepository, scopeStore, httpClientFactory, logger, cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<IResult> GetAll(
        HttpContext httpContext,
        IRequestThrottle requestThrottle,
        IClientRepository clientRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var result = await clientRepository.GetAll(cancellationToken).ConfigureAwait(false);
        return UiEndpointHelpers.ViewOrJson(httpContext, GetAllClientsView, result);
    }

    internal static async Task<IResult> Search(
        HttpContext httpContext,
        SearchClientsRequest? request,
        IRequestThrottle requestThrottle,
        IClientRepository clientRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (request == null)
        {
            return BuildError(CreateLogger(httpContext.RequestServices.GetRequiredService<ILoggerFactory>()), ErrorCodes.InvalidRequest, "no parameter in body request", HttpStatusCode.BadRequest);
        }

        var result = await clientRepository.Search(request, cancellationToken).ConfigureAwait(false);
        return Results.Ok(result);
    }

    internal static async Task<IResult> Get(
        HttpContext httpContext,
        string id,
        IRequestThrottle requestThrottle,
        IClientRepository clientRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var logger = CreateLogger(httpContext.RequestServices.GetRequiredService<ILoggerFactory>());
        if (string.IsNullOrWhiteSpace(id))
        {
            return BuildError(logger, ErrorCodes.InvalidRequest, "identifier is missing", HttpStatusCode.BadRequest);
        }

        var result = await clientRepository.GetById(id, cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            return BuildError(logger, ErrorCodes.InvalidRequest, SharedStrings.TheClientDoesntExist, HttpStatusCode.NotFound);
        }

        return UiEndpointHelpers.ViewOrJson(httpContext, GetClientView, result);
    }

    internal static async Task<IResult> Delete(
        HttpContext httpContext,
        string id,
        IRequestThrottle requestThrottle,
        IClientRepository clientRepository,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var logger = CreateLogger(httpContext.RequestServices.GetRequiredService<ILoggerFactory>());
        if (string.IsNullOrWhiteSpace(id))
        {
            return BuildError(logger, ErrorCodes.InvalidRequest, Strings.IdentifierIsMissing, HttpStatusCode.BadRequest);
        }

        if (!await clientRepository.Delete(id, cancellationToken).ConfigureAwait(false))
        {
            return Results.BadRequest(
                new ErrorDetails
                {
                    Detail = Strings.CouldNotDeleteClient,
                    Status = HttpStatusCode.BadRequest,
                    Title = Strings.DeleteFailed
                });
        }

        return Results.NoContent();
    }

    internal static async Task<IResult> Put(
        HttpContext httpContext,
        Client? client,
        IRequestThrottle requestThrottle,
        IClientRepository clientRepository,
        IScopeStore scopeStore,
        [FromServices] IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        if (client == null)
        {
            return BuildError(CreateLogger(loggerFactory), ErrorCodes.InvalidRequest, Strings.NoParameterInBodyRequest, HttpStatusCode.BadRequest);
        }

        return await PutInternal(client, clientRepository, scopeStore, httpClientFactory, CreateLogger(loggerFactory), cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<IResult> Add(
        HttpContext httpContext,
        Client client,
        IRequestThrottle requestThrottle,
        IClientRepository clientRepository,
        IScopeStore scopeStore,
        [FromServices] IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var logger = CreateLogger(loggerFactory);
        var factory = new ClientFactory(httpClientFactory, scopeStore, DeserializeUris, logger);
        var option = await factory.Build(client, cancellationToken: cancellationToken).ConfigureAwait(false);
        switch (option)
        {
            case Option<Client>.Error e:
                return Results.BadRequest(e.Details);
            case Option<Client>.Result r:
                var result = await clientRepository.Insert(r.Item, cancellationToken).ConfigureAwait(false);
                return result ? Results.Ok(r.Item) : Results.BadRequest();
            default:
                throw new InvalidOperationException();
        }
    }

    internal static async Task<IResult> Create(
        HttpContext httpContext,
        IRequestThrottle requestThrottle)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        return UiEndpointHelpers.ViewOrJson(httpContext, CreateClientView, new CreateClientViewModel());
    }

    internal static async Task<IResult> CreatePost(
        HttpContext httpContext,
        IRequestThrottle requestThrottle,
        IClientRepository clientRepository,
        IScopeStore scopeStore,
        [FromServices] IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var throttled = await EndpointHandlerHelpers.TryThrottleAsync(httpContext, requestThrottle).ConfigureAwait(false);
        if (throttled != null)
        {
            return throttled;
        }

        var viewModel = await EndpointHandlerHelpers.BindFromFormAsync<CreateClientViewModel>(httpContext.Request).ConfigureAwait(false);
        if (viewModel.Name == null || viewModel.RedirectionUrls == null)
        {
            return Results.BadRequest();
        }

        var client = new Client
        {
            ClientName = viewModel.Name,
            LogoUri = viewModel.LogoUri,
            ApplicationType = viewModel.ApplicationType ?? ApplicationTypes.Web,
            RedirectionUrls = viewModel.RedirectionUrls.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries).Select(x => new Uri(x)).ToArray(),
            GrantTypes = viewModel.GrantTypes.ToArray()
        };

        var logger = CreateLogger(loggerFactory);
        var factory = new ClientFactory(httpClientFactory, scopeStore, DeserializeUris, logger);
        var toInsert = await factory.Build(client, cancellationToken: cancellationToken).ConfigureAwait(false);
        switch (toInsert)
        {
            case Option<Client>.Error e:
                return UiEndpointHelpers.RedirectToError(e.Details.Detail, e.Details.Status.ToString(), e.Details.Title);
            case Option<Client>.Result r:
                var result = await clientRepository.Insert(r.Item, cancellationToken).ConfigureAwait(false);
                return result ? Results.Redirect($"/{CoreConstants.EndPoints.Clients}/{r.Item.ClientId}") : Results.BadRequest();
            default:
                throw new InvalidOperationException();
        }
    }

    private static async Task<IResult> PutInternal(
        Client client,
        IClientRepository clientRepository,
        IScopeStore scopeStore,
        IHttpClientFactory httpClientFactory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var clientFactory = new ClientFactory(httpClientFactory, scopeStore, DeserializeUris, logger);
            var toInsert = await clientFactory.Build(client, false, cancellationToken).ConfigureAwait(false);
            switch (toInsert)
            {
                case Option<Client>.Error error:
                    return BuildError(logger, error.Details.Title, error.Details.Detail, error.Details.Status);
                case Option<Client>.Result result:
                    var updated = await clientRepository.Update(result.Item, cancellationToken).ConfigureAwait(false);
                    return updated switch
                    {
                        Option.Success => Results.Ok(result.Item),
                        _ => Results.BadRequest(
                            new ErrorDetails
                            {
                                Status = HttpStatusCode.BadRequest,
                                Title = ErrorCodes.UnhandledExceptionCode,
                                Detail = Strings.RequestIsNotValid
                            })
                    };
                default:
                    throw new InvalidOperationException();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to update client");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static IResult BuildError(ILogger logger, string code, string message, HttpStatusCode statusCode)
    {
        logger.LogError("Error with client, {Error}", message);
        var error = new ErrorDetails { Title = code, Detail = message, Status = statusCode };
        return Results.Json(error, statusCode: (int)statusCode);
    }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger("DotAuth.Controllers.ClientsController");
    }

    private static Uri[] DeserializeUris(string value)
    {
        return JsonSerializer.Deserialize<Uri[]>(value, SharedSerializerContext.Default.UriArray)!;
    }

    private static string BuildAbsolutePath(HttpContext httpContext, string path)
    {
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}{path}";
    }
}




