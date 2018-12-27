namespace SimpleIdentityServer.Manager.Core.Tests.Validators
{
    using System;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Parameters;
    using SimpleAuth.Validators;
    using Xunit;

    public class AddUserParameterValidatorFixture
    {
        private AddUserParameterValidator _addUserParameterValidator;

        [Fact]
        public void When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            Assert.Throws<ArgumentNullException>(() => _addUserParameterValidator.Validate(null));
        }

        [Fact]
        public void When_Pass_No_Login_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var exception = Assert
                .Throws<IdentityServerException>(() =>
                    _addUserParameterValidator.Validate(new AddUserParameter(null, null)));

                        Assert.NotNull(exception);
            Assert.Equal("invalid_request", exception.Code);
            Assert.Equal("the parameter login is missing", exception.Message);
        }

        [Fact]
        public void When_Pass_No_Password_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var exception = Assert.Throws<IdentityServerException>(() =>
                    _addUserParameterValidator.Validate(new AddUserParameter("login", null)));

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
