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

namespace SimpleAuth.Extensions
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Results;
    using Shared.Requests;
    using SimpleAuth.Shared;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    internal static class ControllerExtensions
    {
        public static string GetClientId(this ControllerBase controller)
        {
            if (controller.User?.Identity == null || !controller.User.Identity.IsAuthenticated)
            {
                return string.Empty;
            }

            var claim = controller.User.Claims.FirstOrDefault(c => c.Type == StandardClaimNames.Azp);// || c.Type == StandardTokenRequestParameterNames.ClientIdName);
            return claim == null ? string.Empty : claim.Value;
        }

        public static string GetOriginUrl(this Controller controller)
        {
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
            this ControllerBase controller,
            EndpointResult endpointResult,
            AuthorizationRequest authorizationRequest)
        {
            if (endpointResult.Type == ActionResultType.RedirectToCallBackUrl)
            {
                var parameters = endpointResult.GetRedirectionParameters();
                //var uri = new Uri();
                var redirectUrl = controller.CreateRedirectHttp(
                    authorizationRequest.redirect_uri,
                    parameters,
                    endpointResult.RedirectInstruction.ResponseMode);
                return new RedirectResult(redirectUrl);
            }

            var actionInformation = endpointResult.GetControllerAndActionFromRedirectionActionResult();
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
            this ControllerBase controller,
            Uri uri,
            RouteValueDictionary parameters,
            string responseMode)
        {
            switch (responseMode)
            {
                case ResponseModes.Fragment:
                    uri = uri.AddParametersInFragment(parameters);
                    break;
                case ResponseModes.Query:
                    uri = uri.AddParametersInQuery(parameters);
                    break;
                case ResponseModes.None:
                    break;
                case ResponseModes.FormPost:
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
        private static string CreateRedirectHttp(
            this ControllerBase controller,
            Uri uri,
            RouteValueDictionary parameters,
            string responseMode)
        {
            switch (responseMode)
            {
                case ResponseModes.Fragment:
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