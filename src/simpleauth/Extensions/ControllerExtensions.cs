// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using ResponseMode = SimpleAuth.Parameters.ResponseMode;

namespace SimpleAuth.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;
    using Parsers;
    using Results;
    using Shared.Requests;

    public static class ControllerExtensions
    {
        public static AuthenticationHeaderValue GetAuthenticationHeader(this Controller controller)
        {
            const string authorizationName = "Authorization";
            if (!controller.Request.Headers.TryGetValue(authorizationName, out StringValues values))
            {
                return null;
            }

            var authorizationHeader = values.First();
            return AuthenticationHeaderValue.Parse(authorizationHeader);
        }

        public static string GetClientId(this Controller controller)
        {
            if (controller.User?.Identity == null || !controller.User.Identity.IsAuthenticated)
            {
                return string.Empty;
            }

            var claim = controller.User.Claims.FirstOrDefault(c => c.Type == "client_id");
            if (claim == null)
            {
                return string.Empty;
            }

            return claim.Value;
        }

        public static IEnumerable<Claim> GetClaims(this Controller controller)
        {
            if (controller.User?.Identity == null || !controller.User.Identity.IsAuthenticated)
            {
                return new List<Claim>();
            }

            return controller.User.Claims;
        }

        public static ActionResult GetActionResult(this Controller controller, ApiActionResult result)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }


            if (!string.IsNullOrWhiteSpace(result.Location))
            {
                controller.HttpContext.Response.Headers[HeaderNames.Location] = result.Location;
            }

            if (result.Content != null)
            {
                var res = new ObjectResult(result.Content)
                {
                    StatusCode = result.StatusCode
                };
                return res;
            }

            return new StatusCodeResult(result.StatusCode.Value);
        }

        public static string GetOriginUrl(this Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (!controller.Request.Headers.ContainsKey("Referer"))
            {
                return null;
            }

            var referer = controller.Request.Headers["Referer"];
            var uri = new Uri(referer);
            return $"{uri.Scheme}://{uri.Authority}";
        }

        public static Task DisplayInternalHtml(this Controller controller, string resourceName, Func<string, string> manipulateHtmlCallback = null)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrWhiteSpace(resourceName))
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            var html = GetHtml(resourceName);
            if (manipulateHtmlCallback != null)
            {
                html = manipulateHtmlCallback(html);
            }

            controller.Response.ContentType = "text/html; charset=UTF-8";
            var payload = Encoding.UTF8.GetBytes(html);
            return controller.Response.Body.WriteAsync(payload, 0, payload.Length);
        }

        public static ActionResult CreateRedirectionFromActionResult(
            this Controller controller,
            EndpointResult endpointResult,
            AuthorizationRequest authorizationRequest)
        {
            var actionResultParser = ActionResultParserFactory.CreateActionResultParser();
            if (endpointResult.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var parameters = actionResultParser.GetRedirectionParameters(endpointResult);
                //var uri = new Uri();
                var redirectUrl = controller.CreateRedirectHttp(
                    authorizationRequest.RedirectUri,
                    parameters,
                    endpointResult.RedirectInstruction.ResponseMode);
                return new RedirectResult(redirectUrl);
            }

            var actionInformation = actionResultParser.GetControllerAndActionFromRedirectionActionResult(endpointResult);
            if (actionInformation != null)
            {
                var routeValueDic = actionInformation.RouteValueDictionary;
                routeValueDic.Add("controller", actionInformation.ControllerName);
                routeValueDic.Add("action", actionInformation.ActionName);
                routeValueDic.Add("area", actionInformation.Area);
                return new RedirectToRouteResult(routeValueDic);
            }

            return null;
        }

        public static RedirectResult CreateRedirectHttpTokenResponse(
            this Controller controller,
            Uri uri,
            RouteValueDictionary parameters,
            ResponseMode responseMode)
        {
            switch (responseMode)
            {
                case ResponseMode.fragment:
                    uri = uri.AddParametersInFragment(parameters);
                    break;
                case ResponseMode.query:
                    uri = uri.AddParametersInQuery(parameters);
                    break;
                case ResponseMode.None:
                    break;
                case ResponseMode.form_post:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(responseMode), responseMode, null);
            }

            return new RedirectResult(uri.AbsoluteUri);
        }

        /// <summary>
        /// CreateJwk a redirection HTTP response message based on the response mode.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <param name="responseMode"></param>
        /// <returns></returns>
        public static string CreateRedirectHttp(
            this Controller controller,
            Uri uri,
            RouteValueDictionary parameters,
            ResponseMode responseMode)
        {
            switch (responseMode)
            {
                case ResponseMode.fragment:
                    uri = uri.AddParametersInFragment(parameters);
                    break;
                default:
                    uri = uri.AddParametersInQuery(parameters);
                    break;
            }

            return uri.ToString();
        }

        private static string GetHtml(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string html;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
            }

            return html;
        }
    }
}