using SimpleIdentityServer.Core.Common.Models;

namespace SimpleIdentityServer.TwoFactorAuthentication.Email.Integration.Tests
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;

    public class DefaultEmailServiceTest
    {
        private const string _userName = "";
        private const string _password = "";

        [Fact]
        public async Task When_Send_ConfirmationCode_And_Use_No_Auth_Then_Email_Is_Received()
        {
            // ARRANGE
            var opts = new TwoFactorEmailOptions
            {
                EmailSmtpHost = "aspmx.l.google.com",
                EmailSmtpPort = 25,
                AuthenticationType = AuthenticationTypes.None,
                EmailUserName = _userName,
                EmailPassword = _password,
                EmailBody = "testbody {0}",
                EmailSubject = "sub",
                EmailFromAddress = _userName,
                EmailFromName = "habarthierry@gmail.com"
            };
            var service = new DefaultEmailService(opts);
            var resourceOwner = new ResourceOwner
            {
                Claims = new List<Claim>
                {
                    new Claim("name", "thabart"),
                    new Claim("email", "habarthierry@gmail.com")
                }
            };

            // ACT
            await service.SendAsync("code", resourceOwner).ConfigureAwait(false);

            // ASSERT
            string s = "";
        }

        [Fact]
        public async Task When_Send_ConfirmationCode_And_Use_SSL_Auth_Then_Email_Is_Received()
        {
            // ARRANGE
            var opts = new TwoFactorEmailOptions
            {
                EmailSmtpHost = "smtp.gmail.com",
                EmailSmtpPort = 465,
                AuthenticationType = AuthenticationTypes.SSL,
                EmailUserName = _userName,
                EmailPassword = _password,
                EmailBody = "testbody {0}",
                EmailSubject = "sub",
                EmailFromAddress = _userName,
                EmailFromName = "habarthierry@gmail.com"
            };
            var service = new DefaultEmailService(opts);
            var resourceOwner = new ResourceOwner
            {
                Claims = new List<Claim>
                {
                    new Claim("name", "thabart"),
                    new Claim("email", "habarthierry@gmail.com")
                }
            };

            // ACT
            await service.SendAsync("code", resourceOwner).ConfigureAwait(false);

            // ASSERT
            string s = "";
        }

        [Fact]
        public async Task When_Send_ConfirmationCode_And_Use_TLS_Auth_Then_Email_Is_Received()
        {
            // ARRANGE
            var opts = new TwoFactorEmailOptions
            {
                EmailSmtpHost = "smtp.gmail.com",
                EmailSmtpPort = 587,
                AuthenticationType = AuthenticationTypes.TLS,
                EmailUserName = _userName,
                EmailPassword = _password,
                EmailBody = "testbody {0}",
                EmailSubject = "sub",
                EmailFromAddress = _userName,
                EmailFromName = "habarthierry@gmail.com"
            };
            var service = new DefaultEmailService(opts);
            var resourceOwner = new ResourceOwner
            {
                Claims = new List<Claim>
                {
                    new Claim("name", "thabart"),
                    new Claim("email", "habarthierry@gmail.com")
                }
            };

            // ACT
            await service.SendAsync("code", resourceOwner).ConfigureAwait(false);

            // ASSERT
            string s = "";
        }
    }
}
