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

namespace SimpleAuth.Shared.Errors
{
    internal static class ErrorCodes
    {
        public const string InvalidResourceSetId = "invalid_resource_set_id";
        public const string ExpiredTicket = "expired_ticket";
        public const string ExpiredAuthorizationCode = "expired_code";
        public const string RequestSubmitted = "request_submitted";
        public const string RequestDenied = "request_denied";
        public const string UnhandledExceptionCode = "unhandled_error";
        public const string InvalidParameterCode = "invalid_parameter";
        public const string InvalidRequest = "invalid_request";
        public const string InvalidClient = "invalid_client";
        public const string InvalidGrant = "invalid_grant";
        public const string InvalidCredentials = "invalid_credentials";
        public const string InvalidResponse = "invalid_response";
        public const string InvalidToken = "invalid_token";
        public const string InvalidScope = "invalid_scope";
        public const string InvalidRequestUriCode = "invalid_request_uri";
        public const string LoginRequiredCode = "login_required";
        public const string InteractionRequiredCode = "interaction_required";
        public const string InvalidRedirectUri = "invalid_redirect_uri";
        public const string InvalidUri = "invalid_uri";
        public const string InvalidClientMetaData = "invalid_client_metadata";
        public const string InternalError = "internal_error";
        public const string ClaimRequired = "claim_required";
        public const string RevokeFailed = "revoke_failed";
    }
}
