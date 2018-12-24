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

using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SimpleIdentityServer.Core.Protector
{
    using Shared;

    public class Compressor : ICompressor
    {
        public string Compress(string textToCompress)
        {
            if (string.IsNullOrWhiteSpace(textToCompress))
            {
                throw new ArgumentNullException(nameof(textToCompress));
            }
            
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(textToCompress)))
            {
                using (var compressStream = new MemoryStream())
                {
                    using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
                    {
                        input.CopyTo(compressor);
                        compressor.Dispose();
                        var compressedBytes = compressStream.ToArray();
                        return Convert.ToBase64String(compressedBytes);
                    }
                }
            }
        }

        public string Decompress(string compressedText)
        {
            if (string.IsNullOrWhiteSpace(compressedText))
            {
                throw new ArgumentNullException("compressedText");
            }

            var compressedBytes = compressedText.Base64DecodeBytes();
            using (var input = new MemoryStream(compressedBytes))
            {
                using (var decompressStream = new MemoryStream())
                {
                    using (var decompressor = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        decompressor.CopyTo(decompressStream);
                        decompressor.Dispose();
                        var decompressedBytes = decompressStream.ToArray();
                        return Encoding.UTF8.GetString(decompressedBytes);
                    }
                }
            }
        }
    }
}
