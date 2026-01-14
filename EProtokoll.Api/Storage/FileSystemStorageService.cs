using System.Security.Cryptography;
using EProtokoll.Api.Models;
using Microsoft.Extensions.Options;

namespace EProtokoll.Api.Storage;

public interface IStorageService
{
    Task<(string storageKey, string hash, long size, bool isEncrypted)> SaveAsync(Stream stream, bool encrypt);
    Task<Stream?> OpenReadAsync(string storageKey, bool isEncrypted);
    Task DeleteAsync(string storageKey);
}

public class FileSystemStorageService : IStorageService
{
    private readonly StorageOptions _options;

    public FileSystemStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        Directory.CreateDirectory(_options.RootPath);
    }

    public async Task<(string storageKey, string hash, long size, bool isEncrypted)> SaveAsync(Stream stream, bool encrypt)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        var bytes = memory.ToArray();
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        var storageKey = hash.ToLowerInvariant();
        var path = Path.Combine(_options.RootPath, storageKey);
        if (!File.Exists(path))
        {
            var data = encrypt ? Encrypt(bytes) : bytes;
            await File.WriteAllBytesAsync(path, data);
        }
        return (storageKey, hash, bytes.LongLength, encrypt);
    }

    public Task<Stream?> OpenReadAsync(string storageKey, bool isEncrypted)
    {
        var path = Path.Combine(_options.RootPath, storageKey);
        if (!File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = File.OpenRead(path);
        if (!isEncrypted)
        {
            return Task.FromResult<Stream?>(stream);
        }

        return Task.FromResult<Stream?>(DecryptStream(stream));
    }

    public Task DeleteAsync(string storageKey)
    {
        var path = Path.Combine(_options.RootPath, storageKey);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        return Task.CompletedTask;
    }

    private byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = DeriveKey();
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    private Stream DecryptStream(Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        var allBytes = ms.ToArray();
        using var aes = Aes.Create();
        aes.Key = DeriveKey();
        var iv = allBytes.Take(aes.BlockSize / 8).ToArray();
        var cipher = allBytes.Skip(aes.BlockSize / 8).ToArray();
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var output = new MemoryStream();
        using var cs = new CryptoStream(new MemoryStream(cipher), decryptor, CryptoStreamMode.Read);
        cs.CopyTo(output);
        output.Position = 0;
        return output;
    }

    private byte[] DeriveKey()
    {
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(_options.EncryptionKey);
        return SHA256.HashData(keyBytes);
    }
}
