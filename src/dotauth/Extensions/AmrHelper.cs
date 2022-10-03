namespace DotAuth.Extensions;

using System.Linq;
using System.Net;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;

internal static class AmrHelper
{
    public static Option<string> GetAmr(this string[] currentAmrs, string[]? exceptedAmrs = null)
    {
        if (currentAmrs.Length == 0)
        {
            return new Option<string>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InternalError,
                    Detail = Strings.NoActiveAmr,
                    Status = HttpStatusCode.BadRequest
                });
        }

        var amr = CoreConstants.DefaultAmr;
        if (exceptedAmrs != null)
        {
            foreach (var exceptedAmr in exceptedAmrs)
            {
                if (currentAmrs.Contains(exceptedAmr))
                {
                    amr = exceptedAmr;
                    break;
                }
            }
        }

        if (!currentAmrs.Contains(amr))
        {
            return new Option<string>.Error(
                new ErrorDetails
                {
                    Title = ErrorCodes.InternalError,
                    Detail = string.Format(Strings.TheAmrDoesntExist, amr),
                    Status = HttpStatusCode.BadRequest
                });
        }

        return new Option<string>.Result(amr);
    }
}