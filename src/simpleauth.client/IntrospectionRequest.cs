// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the introspection request.
    /// </summary>
    /// <seealso cref="IEnumerable{KeyValuePair}" />
    public class IntrospectionRequest : IEnumerable<KeyValuePair<string, string>>
    {
        public string PatToken { get; }
        private readonly Dictionary<string, string> _form;

        private IntrospectionRequest(Dictionary<string, string> form, string patToken)
        {
            PatToken = patToken;
            _form = form;
        }

        /// <summary>
        /// Creates the specified request.
        /// </summary>
        /// <param name="rptToken">The rpt token to introspect.</param>
        /// <param name="tokenType">Type of the rptToken.</param>
        /// <param name="patToken">The PAT authorization token for the request.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">rptToken</exception>
        public static IntrospectionRequest Create(string rptToken, string tokenType, string patToken)
        {
            if (string.IsNullOrWhiteSpace(rptToken))
            {
                throw new ArgumentNullException(nameof(rptToken));
            }

            var dict = new Dictionary<string, string> { { "token", rptToken }, { "token_type_hint", tokenType } };

            return new IntrospectionRequest(dict, patToken);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _form.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
