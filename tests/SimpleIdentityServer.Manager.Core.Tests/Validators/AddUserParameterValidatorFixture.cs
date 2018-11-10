using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Validators;
using System;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Validators
{
    public class AddUserParameterValidatorFixture
    {
        private IAddUserParameterValidator _addUserParameterValidator;

        [Fact]
        public void When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _addUserParameterValidator.Validate(null));
        }

        [Fact]
        public void When_Pass_No_Login_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var exception = Assert.Throws<IdentityServerManagerException>(() => _addUserParameterValidator.Validate(new AddUserParameter(null, null)));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal("invalid_request", exception.Code);
            Assert.Equal("the parameter login is missing", exception.Message);
        }

        [Fact]
        public void When_Pass_No_Password_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var exception = Assert.Throws<IdentityServerManagerException>(() => _addUserParameterValidator.Validate(new AddUserParameter("login", null)));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal("invalid_request", exception.Code);
            Assert.Equal("the parameter password is missing", exception.Message);
        }

        private void InitializeFakeObjects()
        {
            _addUserParameterValidator = new AddUserParameterValidator();
        }
    }
}
