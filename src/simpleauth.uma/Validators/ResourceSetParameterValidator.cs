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

namespace SimpleAuth.Uma.Validators
{
    using System;
    using System.Linq;
    using Errors;
    using Exceptions;
    using Models;

    internal class ResourceSetParameterValidator : IResourceSetParameterValidator
    {
        public void CheckResourceSetParameter(ResourceSet resourceSet)
        {
            if (resourceSet == null)
            {
                throw new ArgumentNullException(nameof(resourceSet));
            }

            if (string.IsNullOrWhiteSpace(resourceSet.Name))
            {
                throw new BaseUmaException(UmaErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"));
            }

            if (resourceSet.Scopes == null ||
                !resourceSet.Scopes.Any())
            {
                throw new BaseUmaException(UmaErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "scopes"));
            }

            if (!string.IsNullOrWhiteSpace(resourceSet.IconUri) &&
                !Uri.IsWellFormedUriString(resourceSet.IconUri, UriKind.Absolute))
            {
                throw new BaseUmaException(UmaErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, resourceSet.IconUri));
            }

            if (!string.IsNullOrWhiteSpace(resourceSet.Uri) &&
                !Uri.IsWellFormedUriString(resourceSet.Uri, UriKind.Absolute))
            {
                throw new BaseUmaException(UmaErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, resourceSet.Uri));
            }
        }
    }
}
