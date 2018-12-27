namespace SimpleAuth.Validators
{
    using System;
    using Errors;
    using Exceptions;
    using Parameters;

    internal sealed class AddUserParameterValidator
    {
        public void Validate(AddUserParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(parameter.Login))
            {
                throw new IdentityServerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterIsMissing, "login"));
            }

            if (string.IsNullOrWhiteSpace(parameter.Password))
            {
                throw new IdentityServerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterIsMissing, "password"));
            }
        }
    }
}
