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

namespace SimpleIdentityServer.Shell.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ViewModels;

    [Area("Shell")]
    public class FormController : Controller
    {
        public ActionResult Index(dynamic parameters)
        {
            var queryStringValue = Request.QueryString.Value;
            var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryStringValue);
            var viewModel = new FormViewModel();    
            if (queryString.ContainsKey(Core.CoreConstants.StandardAuthorizationResponseNames.AccessTokenName))
            {
                viewModel.AccessToken = queryString[Core.CoreConstants.StandardAuthorizationResponseNames.AccessTokenName];
            }

            if (queryString.ContainsKey(Core.CoreConstants.StandardAuthorizationResponseNames.AuthorizationCodeName))
            {
                viewModel.AuthorizationCode = queryString[Core.CoreConstants.StandardAuthorizationResponseNames.AuthorizationCodeName];
            }

            if (queryString.ContainsKey(Core.CoreConstants.StandardAuthorizationResponseNames.IdTokenName))
            {
                viewModel.IdToken = queryString[Core.CoreConstants.StandardAuthorizationResponseNames.IdTokenName];
            }

            if (queryString.ContainsKey(Core.CoreConstants.StandardAuthorizationResponseNames.StateName))
            {
                viewModel.State = queryString[Core.CoreConstants.StandardAuthorizationResponseNames.StateName];
            }

            if (queryString.ContainsKey("redirect_uri"))
            {
                viewModel.RedirectUri = queryString["redirect_uri"];
            }

            return View(viewModel);
        }
    }
}