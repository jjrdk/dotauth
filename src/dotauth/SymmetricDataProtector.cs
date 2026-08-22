namespace DotAuth;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

internal sealed class SymmetricDataProtector : IDataProtector
{
    private readonly SymmetricAlgorithm _algorithm;

    public SymmetricDataProtector(SymmetricAlgorithm algorithm)
    {
        _algorithm = algorithm;
    }

    /// <inheritdoc />
    public IDataProtector CreateProtector(string purpose)
    {
        return this;
    }

    /// <inheritdoc />
    public byte[] Protect(byte[] plaintext)
    {
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, _algorithm.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(plaintext);
        cs.Flush();
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    /// <inheritdoc />
    public byte[] Unprotect(byte[] protectedData)
    {
        using var ms = new MemoryStream(protectedData);
        using var cs = new CryptoStream(ms, _algorithm.CreateDecryptor(), CryptoStreamMode.Read);
        var result = new List<byte>();
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        try
        {
            while (true)
            {
                var read = cs.Read(buffer, 0, 4096);
                if (read == 0)
                {
                    break;
                }

                result.AddRange(buffer.AsSpan(0, read));
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        return [.. result];
    }
}
