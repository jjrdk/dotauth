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

namespace SimpleAuth.Uma.Tests.Validators
{
    using Exceptions;
    using Models;
    using SimpleAuth.Errors;
    using System;
    using System.Collections.Generic;
    using Uma.Validators;
    using Xunit;
    using ErrorDescriptions = Errors.ErrorDescriptions;

    public class ResourceSetParameterValidatorFixture
    {
        private ResourceSetParameterValidator _resourceSetParameterValidator;

        [Fact]
        public void When_Passing_Null_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            Assert.Throws<ArgumentNullException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(null));
        }

        [Fact]
        public void When_Name_Is_Not_Pass_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var addResourceParameter = new ResourceSet();

            var exception = Assert.Throws<BaseUmaException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"));
        }

        [Fact]
        public void When_Scopes_Are_Not_Specified_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var addResourceParameter = new ResourceSet
            {
                Name = "name"
            };

            var exception = Assert.Throws<BaseUmaException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "scopes"));
        }

        [Fact]
        public void When_Icon_Uri_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string iconUri = "#icon_uri";
            var addResourceParameter = new ResourceSet
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = iconUri
            };

            var exception = Assert.Throws<BaseUmaException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, iconUri));
        }

        [Fact]
        public void When_Uri_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string uri = "#uri";
            var addResourceParameter = new ResourceSet
            {
                Name = "name",
                Scopes = new List<string> { "scope" },
                IconUri = "http://localhost",
                Uri = uri
            };

            var exception = Assert.Throws<BaseUmaException>(() => _resourceSetParameterValidator.CheckResourceSetParameter(addResourceParameter));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, uri));
        }

        private void InitializeFakeObjects()
        {
            _resourceSetParameterValidator = new ResourceSetParameterValidator();
        }
    }
}
