namespace SimpleAuth.Validators
{
    using System;
    using Errors;
    using Exceptions;
    using Parameters;

    internal sealed class RevokeTokenParameterValidator : IRevokeTokenParameterValidator
    {
        public void Validate(RevokeTokenParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            // Read this RFC for more information
            if (string.IsNullOrWhiteSpace(parameter.Token))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, CoreConstants.IntrospectionRequestNames.Token));
            }
        }
    }
}
