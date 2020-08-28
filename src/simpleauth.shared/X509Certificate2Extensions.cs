namespace SimpleAuth.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;

    public static class X509Certificate2Extensions
    {
        private const string SubjectAlternateNameOID = "2.5.29.17";

        private static readonly Regex DnsNameRegex = new Regex(
            @"^DNS Name=(.+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static HashSet<string> GetSubjectAlternativeNames(this X509Certificate2 cert)
        {
            if (cert == null)
            {
                throw new ArgumentNullException(nameof(cert));
            }
            var subjectAlternativeName = cert.Extensions.Cast<X509Extension>()
                .Where(n => n.Oid!.Value == SubjectAlternateNameOID)
                .Select(n => new AsnEncodedData(n.Oid, n.RawData))
                .Select(n => n.Format(true))
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(subjectAlternativeName)
                ? new HashSet<string>()
                : subjectAlternativeName.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => DnsNameRegex.Match(n))
                    .Where(r => r.Success && !string.IsNullOrWhiteSpace(r.Groups[1].Value))
                    .Select(r => r.Groups[1].Value)
                    .ToHashSet();
        }
    }
}