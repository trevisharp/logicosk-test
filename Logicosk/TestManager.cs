using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System;

namespace Logicosk;

public static class TestManager
{
    public static async Task SaveKey()
    {
        using var aes = Aes.Create();
        using var stream = new FileStream(
            "current.key", 
            FileMode.OpenOrCreate, 
            FileAccess.Write
        );

        var keySize = aes.Key.Length + aes.IV.Length;
        var buffer = new byte[keySize];
        buffer[0] = (byte)(aes.Key.Length - 1);
        buffer[1] = (byte)aes.IV.Length;

        await stream.WriteAsync(buffer);
    }

    public static async Task<Aes> LoadKeys()
    {
        if (!File.Exists("current.key"))
            await SaveKey();
        
        var aes = Aes.Create();
        aes.Padding = PaddingMode.PKCS7;

        using var stream = new FileStream(
            "current.key", 
            FileMode.Open, 
            FileAccess.Read
        );
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        var data = buffer.ToArray();

        var key = new byte[data[0] + 1];
        Array.Copy(data, 1, key, 0, key.Length);
        aes.Key = key;

        var iv = new byte[data[1]];
        Array.Copy(data, 2, iv, 0, iv.Length);
        aes.IV = iv;

        return aes;
    }

    public static async Task Save(string path, Test test)
    {
        var json = JsonSerializer.Serialize(test);
        var content = await encrypt(json);
        
        var dir = Directory.CreateTempSubdirectory();
        var output = Path.Combine(dir.FullName, "prezip.test");
        await File.WriteAllTextAsync(output, content);

        if (File.Exists(path))
            File.Delete(path);
        ZipFile.CreateFromDirectory(
            dir.FullName, path, 
            CompressionLevel.SmallestSize, false
        );

        Directory.Delete(dir.FullName, true);
    }

    public static async Task<Test> Open(string path)
    {
        var zip = ZipFile.Open(path, ZipArchiveMode.Read);
        var main = zip.Entries.FirstOrDefault();
        if (main is null)
            return null;
        
        main.ExtractToFile("extracted");
        var content = await File.ReadAllTextAsync("extracted");
        var json = await decrypt(content);
        File.Delete("extracted");
        
        return JsonSerializer.Deserialize<Test>(json);
    }

    private static async Task<string> decrypt(string input)
    {
        var bytes = Convert.FromBase64String(input);
        using var stream = new MemoryStream(bytes);

        using var aes = await LoadKeys();
        using var decriptor = aes.CreateDecryptor();
        using var crypto = new CryptoStream(
            stream, decriptor, CryptoStreamMode.Read
        );

        using var output = new MemoryStream();
        await crypto.CopyToAsync(output);
        
        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static async Task<string> encrypt(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        using var stream = new MemoryStream();

        using var aes = await LoadKeys();
        using var encryptor = aes.CreateEncryptor();
        
        using var crypto = new CryptoStream(
            stream, encryptor, CryptoStreamMode.Write
        );
        await crypto.WriteAsync(bytes);
        crypto.Close();
        
        return Convert.ToBase64String(stream.ToArray());
    }
}