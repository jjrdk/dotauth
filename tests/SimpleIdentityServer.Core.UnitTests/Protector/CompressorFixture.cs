using SimpleIdentityServer.Core.Protector;
using System;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Protector
{
    public sealed class CompressorFixture
    {
        private ICompressor _compressor;

        [Fact]
        public void When_Passing_Empty_Parameter_To_Compress_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        Assert.Throws<ArgumentNullException>(() => _compressor.Compress(null));
        }

        [Fact]
        public void When_Passing_String_To_Compress_Then_Compressed_String_Is_Returned()
        {            InitializeFakeObjects();
            var toCompress = RandomString(300);

                        var result = _compressor.Compress(toCompress);

                        Assert.NotNull(result);
            Assert.True(result.Length < toCompress.Length);
        }

        [Fact]
        public void When_Passing_EmptryString_To_Decompress_Then_Exceptin_Is_Thrown()
        {            InitializeFakeObjects();

                        Assert.Throws<ArgumentNullException>(() => _compressor.Decompress(null));
        }

        [Fact]
        public void When_Decompressing_Then_DecompressedResult_Is_Returned()
        {            InitializeFakeObjects();
            var toCompress = RandomString(300);
            var compressed = _compressor.Compress(toCompress);

                        var result = _compressor.Decompress(compressed);

                        Assert.NotNull(result);
            Assert.True(result == toCompress);
        }

        private void InitializeFakeObjects()
        {
            _compressor = new Compressor();
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
