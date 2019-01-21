namespace SimpleAuth.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;

    internal static class AmrHelper
    {
        public static string GetAmr(this IEnumerable<string> currentAmrs, IEnumerable<string> exceptedAmrs = null)
        {
            if (currentAmrs == null || !currentAmrs.Any())
            {
                throw new SimpleAuthException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.NoActiveAmr);
            }

            var amr = CoreConstants.DEFAULT_AMR;
            if (exceptedAmrs != null)
            {
                foreach(var exceptedAmr in exceptedAmrs)
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
                throw new SimpleAuthException(Errors.ErrorCodes.InternalError, string.Format(Errors.ErrorDescriptions.TheAmrDoesntExist, amr));
            }

            return amr;
        }
    }
}
