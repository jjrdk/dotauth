namespace SimpleAuth.Protector
{
    public interface ICompressor
    {
        string Compress(string textToCompress);

        string Decompress(string compressedText);
    }
}