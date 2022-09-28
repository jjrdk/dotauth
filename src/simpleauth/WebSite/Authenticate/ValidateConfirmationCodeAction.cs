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

namespace SimpleAuth.WebSite.Authenticate;

using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleAuth.Shared.Repositories;

internal sealed class ValidateConfirmationCodeAction
{
    private readonly IConfirmationCodeStore _confirmationCodeStore;

    public ValidateConfirmationCodeAction(IConfirmationCodeStore confirmationCodeStore)
    {
        _confirmationCodeStore = confirmationCodeStore;
    }

    public async Task<bool> Execute(string code, string subject, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var confirmationCode = await _confirmationCodeStore.Get(code, subject, cancellationToken).ConfigureAwait(false);
        if (confirmationCode == null)
        {
            return false;
        }

        var expirationDateTime = confirmationCode.IssueAt.AddSeconds(confirmationCode.ExpiresIn);
        if (DateTimeOffset.UtcNow < expirationDateTime)
        {
            return true;
        }

        await _confirmationCodeStore.Remove(code, subject, cancellationToken).ConfigureAwait(false);
        return false;
    }
}