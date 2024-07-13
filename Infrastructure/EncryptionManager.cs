using BackendServices;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class EncryptionManager : IEncryptionManager
    {
        public async Task<string> Decrypt(string key, string iv, string encryptedMessage)
        {
            byte[] _key = PasswordHelper.Base64Decode(key);
            byte[] _iv = PasswordHelper.Base64Decode(iv);

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var _encryptedMessage = PasswordHelper.Base64Decode(encryptedMessage);
            using MemoryStream memoryStream = new(_encryptedMessage);
            using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);

            return await streamReader.ReadToEndAsync();

        }

        public async Task<(string key, string iv, string encryptedMessage)> Encrypt(string message)
        {
            using Aes aes = Aes.Create();
            var key = PasswordHelper.Base64(aes.Key);
            var iv = PasswordHelper.Base64(aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using StreamWriter streamWriter = new(cryptoStream);
            await streamWriter.WriteAsync(message);
            await streamWriter.FlushAsync();
            streamWriter.Close();

            var encryptedMessage = PasswordHelper.Base64(memoryStream.ToArray());

            return (key, iv, encryptedMessage);
        }
    }
}
