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
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterIsMissing, "login"));
            }

            if (string.IsNullOrWhiteSpace(parameter.Password))
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterIsMissing, "password"));
            }
        }
    }
}
