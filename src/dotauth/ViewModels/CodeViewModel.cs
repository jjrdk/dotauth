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

namespace DotAuth.ViewModels;

using System;
using System.Diagnostics.CodeAnalysis;
using DotAuth.Properties;
using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Defines the code view model.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CodeViewModel
{
    /// <summary>
    /// The resend action
    /// </summary>
    public const string ResendAction = "resend";
    /// <summary>
    /// The submit action
    /// </summary>
    public const string SubmitAction = "submit";

    /// <summary>
    /// Gets or sets the code.
    /// </summary>
    /// <value>
    /// The code.
    /// </value>
    public string? Code { get; set; }

    /// <summary>
    /// Gets or sets the authentication request code.
    /// </summary>
    /// <value>
    /// The authentication request code.
    /// </value>
    public string? AuthRequestCode { get; set; }

    /// <summary>
    /// Gets or sets the name of the claim.
    /// </summary>
    /// <value>
    /// The name of the claim.
    /// </value>
    public string? ClaimName { get; set; }

    /// <summary>
    /// Gets or sets the claim value.
    /// </summary>
    /// <value>
    /// The claim value.
    /// </value>
    public string? ClaimValue { get; set; }

    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    /// <value>
    /// The action.
    /// </value>
    public string? Action { get; set; }

    /// <summary>
    /// Validates the specified model state.
    /// </summary>
    /// <param name="modelState">State of the model.</param>
    /// <exception cref="ArgumentNullException">modelState</exception>
    public void Validate(ModelStateDictionary modelState)
    {
        if (string.IsNullOrWhiteSpace(ClaimName))
        {
            modelState.AddModelError(nameof(ClaimName), Strings.TheClaimMustBeSpecified);
        }

        switch (Action)
        {
            case ResendAction:
            {
                if (string.IsNullOrWhiteSpace(ClaimValue))
                {
                    modelState.AddModelError(nameof(ClaimValue), Strings.TheClaimMustBeSpecified);
                }

                break;
            }
            case SubmitAction:
            {
                if (string.IsNullOrWhiteSpace(Code))
                {
                    modelState.AddModelError(nameof(Code), Strings.TheConfirmationCodeMustBeSpecified);
                }

                break;
            }
        }
    }
}