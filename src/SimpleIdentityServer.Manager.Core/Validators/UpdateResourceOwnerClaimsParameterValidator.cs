using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;

namespace SimpleIdentityServer.Manager.Core.Validators
{
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