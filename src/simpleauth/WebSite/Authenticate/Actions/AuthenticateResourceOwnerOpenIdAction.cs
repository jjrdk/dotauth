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

namespace SimpleAuth.WebSite.Authenticate.Actions
{
    using Common;
    using Extensions;
    using Helpers;
    using Parameters;
    using Results;
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class AuthenticateResourceOwnerOpenIdAction : IAuthenticateResourceOwnerOpenIdAction
    {
        private readonly IAuthenticateHelper _authenticateHelper;

        public AuthenticateResourceOwnerOpenIdAction(IAuthenticateHelper authenticateHelper)
        {
            _authenticateHelper = authenticateHelper;
        }


        /// <summary>
        /// Returns an action result to the controller's action.
        /// 1). Redirect to the consent screen if the user is authenticated AND the request doesn't contain a login prompt.
        /// 2). Do nothing
        /// </summary>
        /// <param name="authorizationParameter">The parameter</param>
        /// <param name="resourceOwnerPrincipal">Resource owner principal</param>
        /// <param name="code">Encrypted parameter</param>
        /// <returns>Action result to the controller's action</returns>
        public async Task<EndpointResult> Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal resourceOwnerPrincipal,
            string code, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            var resourceOwnerIsAuthenticated = resourceOwnerPrincipal.IsAuthenticated();
            var promptParameters = authorizationParameter.Prompt.ParsePrompts();

            // 1).
            if (resourceOwnerIsAuthenticated &&
                promptParameters != null &&
                !promptParameters.Contains(PromptParameter.login))
            {
                var subject = resourceOwnerPrincipal.GetSubject();
                var claims = resourceOwnerPrincipal.Claims.ToList();
                return await _authenticateHelper.ProcessRedirection(authorizationParameter,
                    code,
                    subject,
                    claims, issuerName).ConfigureAwait(false);
            }

            // 2).
            return EndpointResult.CreateAnEmptyActionResultWithNoEffect();
        }
    }
}
