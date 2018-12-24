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

namespace SimpleIdentityServer.Core.Api.Jws
{
    using System;
    using System.Threading.Tasks;
    using Actions;
    using Parameters;
    using Results;

    public class JwsActions : IJwsActions
    {
        private readonly IGetJwsInformationAction _getJwsInformationAction;
        private readonly ICreateJwsAction _createJwsAction;

        public JwsActions(
            IGetJwsInformationAction getJwsInformationAction, 
            ICreateJwsAction createJwsAction)
        {
            _getJwsInformationAction = getJwsInformationAction;
            _createJwsAction = createJwsAction;
        }

        public Task<JwsInformationResult> GetJwsInformation(GetJwsParameter getJwsParameter)
        {
            if (getJwsParameter == null || string.IsNullOrWhiteSpace(getJwsParameter.Jws))
            {
                throw new ArgumentNullException(nameof(getJwsParameter));
            }

            return _getJwsInformationAction.Execute(getJwsParameter);
        }

        public Task<string> CreateJws(CreateJwsParameter createJwsParameter)
        {
            if (createJwsParameter == null)
            {
                throw new ArgumentNullException(nameof(createJwsParameter));
            }

            return _createJwsAction.Execute(createJwsParameter);
        }
    }
}
