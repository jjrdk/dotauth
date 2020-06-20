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
    using System;
    using Microsoft.Extensions.Logging;

    internal static class ControllerExtensions
    {
        public static string GetOriginUrl(this ControllerBase controller)
        {
            if (!controller.Request.Headers.ContainsKey("Referer"))
            {
                return null;
            }

            var referer = controller.Request.Headers["Referer"];
            var uri = new Uri(referer);
            return $"{uri.Scheme}://{uri.Authority}";
        }

        public static ActionResult CreateRedirectionFromActionResult(
            this EndpointResult endpointResult,
            AuthorizationRequest authorizationRequest,
            ILogger logger)
        {
            if (endpointResult.Type == ActionResultType.RedirectToCallBackUrl)
            {
                var parameters = endpointResult.GetRedirectionParameters();
                var redirectUrl = CreateRedirectHttp(
                    authorizationRequest.redirect_uri,
                    parameters,
                    endpointResult.RedirectInstruction.ResponseMode).ToString();
                logger.LogInformation($"Redirection uri: {redirectUrl}");

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
            this Uri uri,
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

        private static Uri CreateRedirectHttp(Uri uri, RouteValueDictionary parameters, string responseMode)
        {
            return responseMode switch
            {
                ResponseModes.Fragment => uri.AddParametersInFragment(parameters),
                _ => uri.AddParametersInQuery(parameters)
            };
        }
    }
}
