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

namespace SimpleAuth.Api.Jwe
{
    using System;
    using System.Threading.Tasks;
    using Actions;
    using Parameters;
    using Results;

    public class JweActions : IJweActions
    {
        private readonly IGetJweInformationAction _getJweInformationAction;
        private readonly ICreateJweAction _createJweAction;

        public JweActions(
            IGetJweInformationAction getJweInformationAction,
            ICreateJweAction createJweAction)
        {
            _getJweInformationAction = getJweInformationAction;
            _createJweAction = createJweAction;
        }

        public Task<JweInformationResult> GetJweInformation(GetJweParameter getJweParameter)
        {
            if (getJweParameter == null)
            {
                throw new ArgumentNullException(nameof(getJweParameter));
            }

            return _getJweInformationAction.ExecuteAsync(getJweParameter);
        }

        public Task<string> CreateJwe(CreateJweParameter createJweParameter)
        {
            if (createJweParameter == null)
            {
                throw new ArgumentNullException(nameof(createJweParameter));
            }

            return _createJweAction.ExecuteAsync(createJweParameter);
        }
    }
}
