// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using MailKit.Net.Smtp;
using MimeKit;
using SimpleIdentityServer.Core.Common.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.TwoFactorAuthentication.Email
{
    public class TwoFactorEmailOptions
    {
        public string EmailFromName { get; set; }
        public string EmailFromAddress { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string EmailSmtpHost { get; set; }
        public int EmailSmtpPort { get; set; }
        public AuthenticationTypes AuthenticationType { get; set; }
        public string EmailUserName { get; set; }
        public string EmailPassword { get; set; }
    }

    public enum AuthenticationTypes
    {
        None = 0,
        TLS= 1,
        SSL = 2
    }

    public class DefaultEmailService : ITwoFactorAuthenticationService
    {
        private readonly TwoFactorEmailOptions _options;
        
        private MailKit.Security.SecureSocketOptions GetSecureSocketOption(AuthenticationTypes authenticationType)
        {
            if (authenticationType == AuthenticationTypes.SSL)
            {
                return MailKit.Security.SecureSocketOptions.SslOnConnect;
            }

            if (authenticationType == AuthenticationTypes.TLS)
            {
                return MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable;
            }

            return MailKit.Security.SecureSocketOptions.Auto;
        }

        public DefaultEmailService(TwoFactorEmailOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            
            _options = options;
        }

        public string RequiredClaim { get { return SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email; } }
        public string Name => "EMAIL";

        public async Task SendAsync(string code, ResourceOwner user)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.Claims == null)
            {
                throw new ArgumentNullException(nameof(user.Claims));
            }

            // 1. Try to fetch the email.
            var emailClaim = user.Claims.FirstOrDefault(c => c.Type == RequiredClaim);
            if (emailClaim == null)
            {
                throw new ArgumentException("the email is not present");
            }

            // 2. Try to fetch the display name.
            string displayName;
            var displayNameClaim = user.Claims.FirstOrDefault(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name);
            if (displayNameClaim == null)
            {
                displayName = user.Id;
            }
            else
            {
                displayName = displayNameClaim.Value;
            }
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.EmailFromName, _options.EmailFromAddress));
            message.To.Add(new MailboxAddress(displayName, emailClaim.Value));
            message.Subject = _options.EmailSubject;
            var bodyBuilder = new BodyBuilder()
            {
                HtmlBody = string.Format(_options.EmailBody, code)
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_options.EmailSmtpHost, _options.EmailSmtpPort, GetSecureSocketOption(_options.AuthenticationType)).ConfigureAwait(false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                if (!string.IsNullOrWhiteSpace(_options.EmailUserName) && !string.IsNullOrWhiteSpace(_options.EmailPassword))
                {
                    await client.AuthenticateAsync(_options.EmailUserName, _options.EmailPassword).ConfigureAwait(false);
                }

                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}
