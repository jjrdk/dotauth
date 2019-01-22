namespace SimpleAuth.Helpers
{
    using Exceptions;
    using System.Linq;

    internal static class AmrHelper
    {
        public static string GetAmr(this string[] currentAmrs, string[] exceptedAmrs = null)
        {
            if (currentAmrs == null || currentAmrs.Length == 0)
            {
                throw new SimpleAuthException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.NoActiveAmr);
            }

            var amr = CoreConstants.DEFAULT_AMR;
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
                throw new SimpleAuthException(Errors.ErrorCodes.InternalError, string.Format(Errors.ErrorDescriptions.TheAmrDoesntExist, amr));
            }

            return amr;
        }
    }
}
