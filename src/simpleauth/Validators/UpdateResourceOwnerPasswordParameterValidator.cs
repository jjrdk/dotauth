namespace SimpleAuth.Validators
{
    using System;
    using Errors;
    using Exceptions;
    using Parameters;

    internal sealed class UpdateResourceOwnerPasswordParameterValidator : IUpdateResourceOwnerPasswordParameterValidator
    {
        public void Validate(UpdateResourceOwnerPasswordParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            if (string.IsNullOrWhiteSpace(parameter.Login))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.MissingParameter, "login"));
            }

            if (string.IsNullOrWhiteSpace(parameter.Password))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.MissingParameter, "password"));
            }
        }
    }
}
