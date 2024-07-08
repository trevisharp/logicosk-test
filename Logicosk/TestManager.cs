using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Logicosk;

public static class TestManager
{
    public static byte[] CreateKey(string seed = null)
    {
        if (seed is null)
            return getRandomKey();
        return getKeysBySeed(seed);
    }

    public static Aes LoadKeys(byte[] keys)
    {
        var aes = Aes.Create();
        aes.Padding = PaddingMode.PKCS7;

        var key = new byte[keys[0]];
        Array.Copy(keys, 2, key, 0, key.Length);
        aes.Key = key;

        var iv = new byte[keys[1]];
        Array.Copy(keys, 2 + key.Length, iv, 0, iv.Length);
        aes.IV = iv;

        return aes;
    }

    public static async Task Save(string path, string seed, Test test)
    {
        var options = new JsonSerializerOptions() {
            PropertyNameCaseInsensitive = true
        };
        var json = JsonSerializer.Serialize(test, options);
        var content = await safeEncrypt(json, seed);
        
        var dir = Directory.CreateTempSubdirectory();
        var output = Path.Combine(dir.FullName, "data.test");
        await File.WriteAllTextAsync(output, content);

        var extraFilesPath = Path.Combine(dir.FullName, "extra");
        Directory.CreateDirectory(extraFilesPath);

        var extraFiles = test.Questions
            .Where(q => q.Image is not null)
            .Select(q => q.Image)
            .Select(q => Path.Combine(test.ResourceFolder, "S1", q));
        
        foreach (var file in extraFiles)
        {
            if (!File.Exists(file))
                continue;
            
            var name = Path.GetFileName(file);
            var dest = Path.Combine(extraFilesPath, name);
            File.Copy(file, dest);
        }

        if (File.Exists(path))
            File.Delete(path);
        ZipFile.CreateFromDirectory(
            dir.FullName, path,
            CompressionLevel.SmallestSize, false
        );

        Directory.Delete(dir.FullName, true);
    }

    public static async Task<Test> Open(string path, string seed)
    {
        var zip = ZipFile.Open(path, ZipArchiveMode.Read);
        ZipArchiveEntry main = null;
        
        if (Directory.Exists("extra"))
            Directory.Delete("extra", true);
        Directory.CreateDirectory("extra");

        foreach (var entry in zip.Entries)
        {
            if (entry.FullName.EndsWith("data.test"))
            {
                main = entry;
                continue;
            }

            if (entry.FullName.EndsWith("/"))
                continue;
            
            entry.ExtractToFile(entry.FullName);
        }
        
        if (main is null)
            return null;
        
        main.ExtractToFile("extracted");
        var content = await File.ReadAllTextAsync("extracted");
        var json = await safeDecrypt(content, seed);
        File.Delete("extracted");
        
        var options = new JsonSerializerOptions() {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<Test>(json, options);
    }

    private static async Task<string> safeEncrypt(string input, string seed)
    {
        var randomKeys = getRandomKey();
        var seedKeys = getKeysBySeed(seed);

        var midResult = await encrypt(input, randomKeys, true);
        var finalResult = await encrypt(midResult, seedKeys, false);

        return finalResult;
    }

    private static async Task<string> safeDecrypt(string input, string seed)
    {
        var seedKeys = getKeysBySeed(seed);

        var midResult = await decrypt(input, seedKeys);
        var finalResult = await decrypt(midResult);

        return finalResult;
    }

    private static byte[] getRandomKey()
    {
        using var aes = Aes.Create();

        var keySize = aes.Key.Length + aes.IV.Length;
        var buffer = new byte[keySize + 2];
        buffer[0] = (byte)aes.Key.Length;
        buffer[1] = (byte)aes.IV.Length;

        Array.Copy(aes.Key, 0, buffer, 2, aes.Key.Length);
        Array.Copy(aes.IV, 0, buffer, 2 + aes.Key.Length, aes.IV.Length);

        return buffer;
    }

    private static byte[] getKeysBySeed(string seed)
    {
        var seedBytes = Encoding.UTF8.GetBytes(seed);
        var hash = SHA256.HashData(seedBytes);

        var buffer = new byte[50];
        buffer[0] = 32;
        buffer[1] = 16;

        Array.Copy(hash, 0, buffer, 2, 32);
        Array.Copy(hash, 16, buffer, 34, 16);
        return buffer;
    }

    private static async Task<string> decrypt(string input, byte[] keyData = null)
    {
        var bytes = Convert.FromBase64String(input);
        using var stream = new MemoryStream(bytes);

        var needLoadKyes = stream.ReadByte() == 1;
        if (!needLoadKyes && keyData is null)
            throw new Exception("Missing keyData");
        
        if (keyData is null)
        {
            int keySize = stream.ReadByte();
            int ivSize = stream.ReadByte();
            keyData = new byte[keySize + ivSize + 2];
            keyData[0] = (byte)keySize;
            keyData[1] = (byte)ivSize;
            await stream.ReadAsync(keyData, 2, keySize + ivSize);
        }

        using var aes = LoadKeys(keyData);
        using var decriptor = aes.CreateDecryptor();
        using var crypto = new CryptoStream(
            stream, decriptor, CryptoStreamMode.Read
        );

        using var output = new MemoryStream();
        await crypto.CopyToAsync(output);
        
        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static async Task<string> encrypt(string input, byte[] keyData, bool saveKeys)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        using var stream = new MemoryStream();

        stream.WriteByte(saveKeys ? (byte)1 : (byte)0);
        if (saveKeys)
            await stream.WriteAsync(keyData);

        using var aes = LoadKeys(keyData);
        using var encryptor = aes.CreateEncryptor();
        
        using var crypto = new CryptoStream(
            stream, encryptor, CryptoStreamMode.Write
        );
        await crypto.WriteAsync(bytes);
        crypto.Close();
        
        return Convert.ToBase64String(stream.ToArray());
    }
}