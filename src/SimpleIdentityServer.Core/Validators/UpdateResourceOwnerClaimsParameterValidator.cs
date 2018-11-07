namespace SimpleIdentityServer.Core.Validators
{
    using System;
    using Errors;
    using Exceptions;
    using Parameters;

    public interface IUpdateResourceOwnerClaimsParameterValidator
    {
        void Validate(UpdateResourceOwnerClaimsParameter parameter);
    }

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
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterIsMissing, "login"));
            }
        }
    }
}