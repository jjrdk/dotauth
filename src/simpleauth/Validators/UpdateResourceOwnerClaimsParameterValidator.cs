namespace SimpleAuth.Validators
{
    using System;
    using Errors;
    using Exceptions;
    using Parameters;

    internal sealed class UpdateResourceOwnerClaimsParameterValidator : IUpdateResourceOwnerClaimsParameterValidator
    {
        public void Validate(UpdateResourceOwnerClaimsParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(parameter.Login))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterIsMissing, "login"));
            }
        }
    }
}