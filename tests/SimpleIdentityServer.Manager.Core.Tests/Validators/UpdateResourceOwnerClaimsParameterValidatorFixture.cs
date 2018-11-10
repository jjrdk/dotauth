using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Validators;
using System;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Validators
{
    public class UpdateResourceOwnerClaimsParameterValidatorFixture
    {
        private IUpdateResourceOwnerClaimsParameterValidator _updateResourceOwnerClaimsParameterValidator;

        [Fact]
        public void When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _updateResourceOwnerClaimsParameterValidator.Validate(null));
        }

        [Fact]
        public void When_Login_Is_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var ex = Assert.Throws<IdentityServerManagerException>(() => _updateResourceOwnerClaimsParameterValidator.Validate(new Parameters.UpdateResourceOwnerClaimsParameter()));

            // ASSERTS
            Assert.NotNull(ex);
            Assert.Equal("invalid_request", ex.Code);
            Assert.Equal("the parameter login is missing", ex.Message);
        }

        private void InitializeFakeObjects()
        {
            _updateResourceOwnerClaimsParameterValidator = new UpdateResourceOwnerClaimsParameterValidator();
        }
    }
}
