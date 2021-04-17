namespace SimpleAuth.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines the X509 certificate extension methods.
    /// </summary>
    public static class X509Certificate2Extensions
    {
        private const string SubjectAlternateNameOid = "2.5.29.17";

        private static readonly Regex DnsNameRegex = new Regex(
            @"^DNS Name=(.+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Gets all alternate subject names in the certificate.
        /// </summary>
        /// <param name="cert">The <see cref="X509Certificate2"/> to get alternate names from.</param>
        /// <returns>A <see cref="HashSet{T}"/> instance with all alternate names.</returns>
        public static HashSet<string> GetSubjectAlternativeNames(this X509Certificate2 cert)
        {
            var subjectAlternativeName = cert.Extensions.Cast<X509Extension>()
                .Where(n => n.Oid!.Value == SubjectAlternateNameOid)
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