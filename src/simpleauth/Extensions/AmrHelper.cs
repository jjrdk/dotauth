namespace SimpleAuth.Extensions
{
    using System.Linq;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;

    internal static class AmrHelper
    {
        public static string GetAmr(this string[] currentAmrs, string[] exceptedAmrs = null)
        {
            if (currentAmrs == null || currentAmrs.Length == 0)
            {
                throw new SimpleAuthException(ErrorCodes.InternalError, ErrorMessages.NoActiveAmr);
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
                throw new SimpleAuthException(ErrorCodes.InternalError, string.Format(ErrorMessages.TheAmrDoesntExist, amr));
            }

            return amr;
        }
    }
}
