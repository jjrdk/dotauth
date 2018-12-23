namespace SimpleIdentityServer.Manager.Core.Tests.Validators
{
    using System;
    using SimpleIdentityServer.Core.Exceptions;
    using SimpleIdentityServer.Core.Parameters;
    using SimpleIdentityServer.Core.Validators;
    using Xunit;

    public class UpdateResourceOwnerClaimsParameterValidatorFixture
    {
        private IUpdateResourceOwnerClaimsParameterValidator _updateResourceOwnerClaimsParameterValidator;

        [Fact]
        public void When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        Assert.Throws<ArgumentNullException>(() => _updateResourceOwnerClaimsParameterValidator.Validate(null));
        }

        [Fact]
        public void When_Login_Is_Null_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        var ex = Assert.Throws<IdentityServerManagerException>(() => _updateResourceOwnerClaimsParameterValidator.Validate(new UpdateResourceOwnerClaimsParameter()));

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
