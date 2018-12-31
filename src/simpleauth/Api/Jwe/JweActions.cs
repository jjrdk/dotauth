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
    using Encrypt;
    using Errors;
    using Exceptions;
    using Helpers;
    using Parameters;
    using Results;
    using Signature;

    public class JweActions : IJweActions
    {
        private readonly IJweGenerator _jweGenerator;
        private readonly IJsonWebKeyHelper _jsonWebKeyHelper;
        private readonly IJwsParser _jwsParser;
        private readonly IJweParser _jweParser;

        public JweActions(
            IJweGenerator jweGenerator,
            IJsonWebKeyHelper jsonWebKeyHelper,
            IJwsParser jwsParser,
            IJweParser jweParser)
        {
            _jweGenerator = jweGenerator;
            _jsonWebKeyHelper = jsonWebKeyHelper;
            _jwsParser = jwsParser;
            _jweParser = jweParser;
        }

        public async Task<JweInformationResult> GetJweInformation(GetJweParameter getJweParameter)
        {
            if (getJweParameter == null)
            {
                throw new ArgumentNullException(nameof(getJweParameter));
            }

            if (string.IsNullOrWhiteSpace(getJweParameter.Jwe))
            {
                throw new ArgumentNullException(nameof(getJweParameter.Jwe));
            }

            if (string.IsNullOrWhiteSpace(getJweParameter.Url))
            {
                throw new ArgumentNullException(nameof(getJweParameter.Url));
            }

            if (!Uri.TryCreate(getJweParameter.Url, UriKind.Absolute, out var uri))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, getJweParameter.Url));
            }

            var jwe = getJweParameter.Jwe;
            var jweHeader = _jweParser.GetHeader(jwe);
            if (jweHeader == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheTokenIsNotAValidJwe);
            }

            var jsonWebKey = await _jsonWebKeyHelper.GetJsonWebKey(jweHeader.Kid, uri).ConfigureAwait(false);
            if (jsonWebKey == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, jweHeader.Kid, uri.AbsoluteUri));
            }

            var content = !string.IsNullOrWhiteSpace(getJweParameter.Password)
                ? _jweParser.ParseByUsingSymmetricPassword(jwe, jsonWebKey, getJweParameter.Password)
                : _jweParser.Parse(jwe, jsonWebKey);

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheContentCannotBeExtractedFromJweToken);
            }

            var result = new JweInformationResult
            {
                Content = content,
                IsContentJws = false
            };

            var jwsHeader = _jwsParser.GetHeader(content);
            if (jwsHeader != null)
            {
                result.IsContentJws = true;
            }

            return result;
        }

        public async Task<string> CreateJwe(CreateJweParameter createJweParameter)
        {
            if (createJweParameter == null)
            {
                throw new ArgumentNullException(nameof(createJweParameter));
            }

            if (string.IsNullOrWhiteSpace(createJweParameter.Url))
            {
                throw new ArgumentNullException(nameof(createJweParameter.Url));
            }

            if (string.IsNullOrWhiteSpace(createJweParameter.Jws))
            {
                throw new ArgumentNullException(nameof(createJweParameter.Jws));
            }

            if (string.IsNullOrWhiteSpace(createJweParameter.Kid))
            {
                throw new ArgumentNullException(nameof(createJweParameter.Kid));
            }

            if (!Uri.TryCreate(createJweParameter.Url, UriKind.Absolute, out var uri))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, createJweParameter.Url));
            }

            var jsonWebKey = await _jsonWebKeyHelper.GetJsonWebKey(createJweParameter.Kid, uri).ConfigureAwait(false);
            if (jsonWebKey == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, createJweParameter.Kid, uri.AbsoluteUri));
            }

            string result;
            if (!string.IsNullOrWhiteSpace(createJweParameter.Password))
            {
                result = _jweGenerator.GenerateJweByUsingSymmetricPassword(createJweParameter.Jws,
                    createJweParameter.Alg,
                    createJweParameter.Enc,
                    jsonWebKey,
                    createJweParameter.Password);
            }
            else
            {
                result = _jweGenerator.GenerateJwe(createJweParameter.Jws,
                    createJweParameter.Alg,
                    createJweParameter.Enc,
                    jsonWebKey);
            }

            return result;
        }
    }
}
