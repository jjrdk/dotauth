namespace SimpleAuth;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

internal sealed class SymmetricDataProtector : IDataProtector
{
    private readonly SymmetricAlgorithm _algo;

    public SymmetricDataProtector(SymmetricAlgorithm algo)
    {
        _algo = algo;
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
        using var cs = new CryptoStream(ms, _algo.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(plaintext);
        cs.Flush();
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    /// <inheritdoc />
    public byte[] Unprotect(byte[] protectedData)
    {
        using var ms = new MemoryStream(protectedData);
        using var cs = new CryptoStream(ms, _algo.CreateDecryptor(), CryptoStreamMode.Read);
        List<byte>? list = null;
        const int bufferLength = 4096;
        var buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
        while (true)
        {
            var read = cs.Read(buffer);
            if (read == 0)
            {
                var result = new byte[read];
                Array.Copy(buffer, 0, result, 0, read);
                ArrayPool<byte>.Shared.Return(buffer);
                if (list == null)
                {
                    return result;
                }

                list.AddRange(result);
                return list.ToArray();
            }

            list ??= new List<byte>();
            list.AddRange(buffer.Take(read));
        }
    }
}