namespace SimpleAuth.Tests.Validators
{
    using System;
    using Errors;
    using Exceptions;
    using Parameters;
    using SimpleAuth.Validators;
    using Xunit;

    public class UpdateResourceOwnerPasswordParameterValidatorFixture
    {
        private IUpdateResourceOwnerPasswordParameterValidator _updateResourceOwnerPasswordParameterValidator;

        [Fact]
        public void When_Pass_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            Assert.Throws<ArgumentNullException>(() => _updateResourceOwnerPasswordParameterValidator.Validate(null));
        }

        [Fact]
        public void When_Pass_No_Login_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var exception = Assert.Throws<SimpleAuthException>(
                () => _updateResourceOwnerPasswordParameterValidator.Validate(
                    new UpdateResourceOwnerPasswordParameter
                    {
                        Login = string.Empty
                    }));

            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal("the parameter login is missing", exception.Message);
        }

        [Fact]
        public void When_Pass_No_Password_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var exception = Assert.Throws<SimpleAuthException>(
                () => _updateResourceOwnerPasswordParameterValidator.Validate(
                    new UpdateResourceOwnerPasswordParameter
                    {
                        Login = "login"
                    }));

            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal("the parameter password is missing", exception.Message);
        }

        private void InitializeFakeObjects()
        {
            _updateResourceOwnerPasswordParameterValidator = new UpdateResourceOwnerPasswordParameterValidator();
        }
    }
}
