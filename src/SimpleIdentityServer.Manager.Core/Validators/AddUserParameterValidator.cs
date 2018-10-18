using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using System;

namespace SimpleIdentityServer.Manager.Core.Validators
{
    public interface IAddUserParameterValidator
    {
        void Validate(AddUserParameter parameter);
    }

    internal sealed class AddUserParameterValidator : IAddUserParameterValidator
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
