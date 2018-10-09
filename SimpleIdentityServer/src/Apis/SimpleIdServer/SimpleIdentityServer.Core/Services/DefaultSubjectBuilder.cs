using System;

namespace SimpleIdentityServer.Core.Services
{
    public class DefaultSubjectBuilder : ISubjectBuilder
    {
        public string BuildSubject()
        {
            return Guid.NewGuid().ToString();
        }
    }
}