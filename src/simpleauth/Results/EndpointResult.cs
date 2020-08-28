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

namespace SimpleAuth.Results
{
    /// <summary>
    /// Represents an endpoint resultKind value.
    /// </summary>
    internal class EndpointResult
    {
        /// <summary>
        /// Gets or sets the type of action resultKind.
        /// </summary>
        public ActionResultType Type { get; set; }

        /// <summary>
        /// Gets or sets the redirect instruction.
        /// </summary>
        /// <value>
        /// The redirect instruction.
        /// </value>
        public RedirectInstruction? RedirectInstruction { get; set; }

        /// <summary>
        /// Gets or sets the process identifier.
        /// </summary>
        /// <value>
        /// The process identifier.
        /// </value>
        public string? ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the amr.
        /// </summary>
        /// <value>
        /// The amr.
        /// </value>
        public string? Amr { get; set; }

        /// <summary>
        /// Creates an empty action resultKind with redirection
        /// </summary>
        /// <returns>Empty action resultKind with redirection</returns>
        public static EndpointResult CreateAnEmptyActionResultWithRedirection()
        {
            return new EndpointResult
            {
                RedirectInstruction = new RedirectInstruction(),
                Type = ActionResultType.RedirectToAction
            };
        }

        /// <summary>
        /// Creates an empty action resultKind with output
        /// </summary>
        /// <returns>Empty action resultKind with output</returns>
        public static EndpointResult CreateAnEmptyActionResultWithOutput()
        {
            return new EndpointResult
            {
                RedirectInstruction = null,
                Type = ActionResultType.Output
            };
        }

        /// <summary>
        /// Creates an empty action resultKind with no effect
        /// </summary>
        /// <returns>Empty action resultKind with no effect</returns>
        public static EndpointResult CreateAnEmptyActionResultWithNoEffect()
        {
            return new EndpointResult
            {
                Type = ActionResultType.None
            };
        }

        /// <summary>
        /// Creates an empty action resultKind with redirection to callbackurl.
        /// </summary>
        /// <returns>Empty action with redirection to callbackurl</returns>
        public static EndpointResult CreateAnEmptyActionResultWithRedirectionToCallBackUrl()
        {
            return new EndpointResult
            {
                Type = ActionResultType.RedirectToCallBackUrl,
                RedirectInstruction = new RedirectInstruction()
            };
        }
    }
}
