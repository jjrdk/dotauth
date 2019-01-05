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

namespace SimpleAuth.Tests.Validators
{
    using System;
    using Errors;
    using Exceptions;
    using Parameters;
    using SimpleAuth;
    using SimpleAuth.Validators;
    using Xunit;

    public class ClientCredentialsGrantTypeParameterValidatorFixture
    {
        private IClientCredentialsGrantTypeParameterValidator _clientCredentialsGrantTypeParameterValidator;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        Assert.Throws<ArgumentNullException>(() => _clientCredentialsGrantTypeParameterValidator.Validate(null));
        }

        [Fact]
        public void When_Scope_Is_Empty_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var parameter = new ClientCredentialsGrantTypeParameter();

                        var exception = Assert.Throws<SimpleAuthException>(() => _clientCredentialsGrantTypeParameterValidator.Validate(parameter));
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, CoreConstants.StandardTokenRequestParameterNames.ScopeName));
        }

        private void InitializeFakeObjects()
        {
            _clientCredentialsGrantTypeParameterValidator = new ClientCredentialsGrantTypeParameterValidator();
        }
    }
}
