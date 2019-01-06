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

namespace SimpleAuth.Api.Introspection
{
    using Actions;
    using Parameters;
    using Results;
    using Shared;
    using System;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Validators;

    public class IntrospectionActions : IIntrospectionActions
    {
        private readonly IPostIntrospectionAction _postIntrospectionAction;
        private readonly IEventPublisher _eventPublisher;
        private readonly IIntrospectionParameterValidator _introspectionParameterValidator;

        public IntrospectionActions(IPostIntrospectionAction postIntrospectionAction,
            IEventPublisher eventPublisher,
            IIntrospectionParameterValidator introspectionParameterValidator)
        {
            _postIntrospectionAction = postIntrospectionAction;
            _eventPublisher = eventPublisher;
            _introspectionParameterValidator = introspectionParameterValidator;
        }

        public async Task<IntrospectionResult> PostIntrospection(IntrospectionParameter introspectionParameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            string issuerName)
        {
            if (introspectionParameter == null)
            {
                throw new ArgumentNullException(nameof(introspectionParameter));
            }

            _introspectionParameterValidator.Validate(introspectionParameter);
            var result = await _postIntrospectionAction
                .Execute(introspectionParameter, authenticationHeaderValue, issuerName)
                .ConfigureAwait(false);
            return result;
        }
    }
}
