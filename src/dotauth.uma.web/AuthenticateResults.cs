namespace DotAuth.Uma.Web;

using Microsoft.AspNetCore.Authentication;

internal static class AuthenticateResults
{
    internal static readonly AuthenticateResult ValidatorNotFound = AuthenticateResult.Fail("No SecurityTokenValidator available for token.");
    internal static readonly AuthenticateResult TokenHandlerUnableToValidate = AuthenticateResult.Fail("No TokenHandler was able to validate the token.");
}
