//namespace SimpleAuth.ResourceServer
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Net;
//    using System.Security.Claims;
//    using System.Threading.Tasks;
//    using Microsoft.AspNetCore.Mvc;
//    using Microsoft.AspNetCore.Mvc.Authorization;
//    using Microsoft.AspNetCore.Mvc.Filters;
//    using Microsoft.Extensions.DependencyInjection;
//    using Microsoft.Extensions.Logging;
//    using Newtonsoft.Json;
//    using SimpleAuth.Client;
//    using SimpleAuth.Shared;
//    using SimpleAuth.Shared.Responses;

//    public class UmaAuthorizeFilterAttribute : AuthorizeFilter
//    {
//        private readonly string _resourceSetId;
//        private readonly ILogger<UmaAuthorizeFilterAttribute> _logger;
//        private readonly HashSet<string> _scopes;

//        public UmaAuthorizeFilterAttribute(string resourceSetId, string[] scopes, ILogger<UmaAuthorizeFilterAttribute> logger)
//        {
//            _resourceSetId = resourceSetId;
//            _logger = logger;
//            _scopes = new HashSet<string>(scopes);
//        }

//        public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
//        {
//            var user = context.HttpContext.User;
//            var ticketJson = user.Claims.FirstOrDefault(x => x.Type == "ticket")?.Value;
//            if (string.IsNullOrWhiteSpace(ticketJson))
//            {
//                await HandleNoTicket(context, user);
//            }
//            else
//            {
//                var ticket = JsonConvert.DeserializeObject<UmaTicket>(ticketJson);
//                var scopes = ticket.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries);

//                if (ticket.ResourceId != _resourceSetId || !_scopes.IsSupersetOf(scopes))
//                {
//                    return new ForbidResult();
//                }
//            }
//        }

//        private async Task HandleNoTicket(AuthorizationFilterContext context, ClaimsPrincipal user)
//        {
//            try
//            {
//                var serviceProvider = context.HttpContext.RequestServices;
//                var client = serviceProvider.GetService<IUmaPermissionClient>();
//                GenericResponse<PermissionResponse> permission = null;
//                    //await client.RequestPermission(
//                    //user.Identity as ClaimsIdentity,
//                    //_resourceSetId,
//                    //_scopes.ToArray());
//                var ticketId = permission.Content?.TicketId;
//                if (!string.IsNullOrWhiteSpace(ticketId))
//                {
//                    var configurationProvider = serviceProvider.GetService<IProvideUmaConfiguration>();
//                    var umaConfiguration = await configurationProvider.GetUmaConfiguration();
//                    context.Result = new UmaTicketResult(umaConfiguration.Issuer, ticketId);
//                }
//                else
//                {
//                    context.Result = new ForbidResult();
//                }
//            }
//            catch (Exception exception)
//            {
//                _logger.LogError(exception, "Failed to get UMA permissions");
//                context.Result = new UmaExceptionResponse();
//            }
//        }

//        private class UmaTicketResult : IActionResult
//        {
//            private readonly string _issuer;
//            private readonly string _ticketId;

//            public UmaTicketResult(string issuer, string ticketId)
//            {
//                _issuer = issuer;
//                _ticketId = ticketId;
//            }

//            public Task ExecuteResultAsync(ActionContext context)
//            {
//                var response = context.HttpContext.Response;
//                response.StatusCode = (int)HttpStatusCode.Unauthorized;
//                response.Headers["WWW-Authenticate"] =
//                    $"UMA, as_uri=\"{_issuer}\", ticket=\"{_ticketId}\"";
//                return Task.CompletedTask;
//            }
//        }

//        private class UmaExceptionResponse : IActionResult
//        {
//            public Task ExecuteResultAsync(ActionContext context)
//            {
//                var response = context.HttpContext.Response;

//                response.StatusCode = (int)HttpStatusCode.Forbidden;
//                response.Headers["Warning"] = "199 - \"UMA Authorization Server Unreachable\"";
//                return Task.CompletedTask;
//            }
//        }
//    }
//}
