namespace SimpleIdentityServer.Core.Protector
{
    public interface ICompressor
    {
        string Compress(string textToCompress);

        string Decompress(string compressedText);
    }
}