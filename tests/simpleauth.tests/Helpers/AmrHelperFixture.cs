namespace SimpleAuth.Tests.Helpers
{
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using System;
    using Xunit;

    public class AmrHelperFixture
    {
        [Fact]
        public void When_No_Amr_Then_Exception_Is_Thrown()
        {
            var exception = Assert.Throws<SimpleAuthException>(() => Array.Empty<string>().GetAmr(new[] {"pwd"}));

            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(ErrorDescriptions.NoActiveAmr, exception.Message);
        }

        [Fact]
        public void When_Amr_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            var exception = Assert.Throws<SimpleAuthException>(() => new[] {"invalid"}.GetAmr(new[] {"pwd"}));

            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheAmrDoesntExist, "pwd"), exception.Message);
        }

        [Fact]
        public void When_Amr_Does_Not_Exist_Then_Default_One_Is_Returned()
        {
            var amr = new[] {"pwd"}.GetAmr(new[] {"invalid"});

            Assert.Equal("pwd", amr);
        }

        [Fact]
        public void When_Amr_Exists_Then_Same_Amr_Is_Returned()
        {
            var amr = new[] {"amr"}.GetAmr(new[] {"amr"});

            Assert.Equal("amr", amr);
        }
    }
}
