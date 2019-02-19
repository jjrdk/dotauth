﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.ViewModels
{
    using System;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class CodeViewModel
    {
        public const string ResendAction = "resend";
        public const string SubmitAction = "submit";

        public string Code { get; set; }
        public string AuthRequestCode { get; set; }
        public string ClaimName { get; set; }
        public string ClaimValue { get; set; }
        public string Action { get; set; }

        public void Validate(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            if (Action == ResendAction)
            {
                if (string.IsNullOrWhiteSpace(ClaimValue))
                {
                    modelState.AddModelError("ClaimValue", "The claim must be specified");
                }
            }

            if (Action == SubmitAction)
            {
                if (string.IsNullOrWhiteSpace(Code))
                {
                    modelState.AddModelError("Code", "The confirmation code must be specified");
                }
            }
        }
    }
}
