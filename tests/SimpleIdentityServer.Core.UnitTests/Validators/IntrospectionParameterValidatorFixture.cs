﻿// Copyright 2015 Habart Thierry
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

using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    public class IntrospectionParameterValidatorFixture
    {
        private IntrospectionParameterValidator _introspectionParameterValidator;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        Assert.Throws<ArgumentNullException>(() => _introspectionParameterValidator.Validate(null));
        }


        [Fact]
        public void When_No_Token_Is_Specified_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var parameter = new IntrospectionParameter();

                        var exception = Assert.Throws<IdentityServerException>(() => _introspectionParameterValidator.Validate(parameter));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, CoreConstants.IntrospectionRequestNames.Token));
        }

        [Fact]
        public void When_Passing_Valid_Parameter_Then_No_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var parameter = new IntrospectionParameter
            {
                Token = "token"
            };

                        var exception = Record.Exception(() => _introspectionParameterValidator.Validate(parameter));

                        Assert.Null(exception);
        }

        private void InitializeFakeObjects()
        {
            _introspectionParameterValidator = new IntrospectionParameterValidator();
        }
    }
}
