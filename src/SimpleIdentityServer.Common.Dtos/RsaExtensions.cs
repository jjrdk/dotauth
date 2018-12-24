// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleIdentityServer.Shared
{
    using System;
    using System.Security.Cryptography;
    using System.Xml.Linq;

    public static class RsaExtensions
    {
        public static string ToXmlString(this RSA rsa, bool includePrivateParameters)
        {
            // From the XMLDSIG spec, RFC 3075, Section 6.4.2,
            var rsaParams = rsa.ExportParameters(includePrivateParameters);
            var doc = new XElement("RSAKeyValue",
                new XElement("Modulus", Convert.ToBase64String(rsaParams.Modulus)),
                new XElement("Exponent", Convert.ToBase64String(rsaParams.Exponent)));
            if (includePrivateParameters)
            {
                doc.Add(
                    new XElement("P", Convert.ToBase64String(rsaParams.P)),
                    new XElement("Q", Convert.ToBase64String(rsaParams.Q)),
                    new XElement("DP", Convert.ToBase64String(rsaParams.DP)),
                    new XElement("DQ", Convert.ToBase64String(rsaParams.DQ)),
                    new XElement("InverseQ", Convert.ToBase64String(rsaParams.InverseQ)),
                    new XElement("D", Convert.ToBase64String(rsaParams.D)));
            }
            return doc.ToString();
        }

        public static void FromXmlString(this RSA rsa, string xmlString)
        {
            var xdoc = XDocument.Parse(xmlString).Root;
            var d = xdoc.Element("D");
            var dp = xdoc.Element("DP");
            var dq = xdoc.Element("DQ");
            var p = xdoc.Element("P");
            var q = xdoc.Element("Q");
            var inverseQ = xdoc.Element("InverseQ");
            var parameters = new RSAParameters
            {
                Modulus = Convert.FromBase64String(xdoc.Element("Modulus").Value),
                Exponent = Convert.FromBase64String(xdoc.Element("Exponent").Value),
                D = d == null ? null : Convert.FromBase64String(d.Value),
                DP = dp == null ? null : Convert.FromBase64String(dp.Value),
                DQ = dq == null ? null : Convert.FromBase64String(dq.Value),
                P = p == null ? null : Convert.FromBase64String(p.Value),
                Q = q == null ? null : Convert.FromBase64String(q.Value),
                InverseQ = inverseQ == null ? null : Convert.FromBase64String(inverseQ.Value),
            };
            rsa.ImportParameters(parameters);
        }
    }
}
