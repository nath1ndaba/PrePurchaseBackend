using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static partial class PasswordHelper
    {
        public static readonly IReadOnlyDictionary<string, PasswordHashAlgorithm> PasswordHasingAlgorithms = new Dictionary<string, PasswordHashAlgorithm>()
        {
            {"argon2id", PasswordHashAlgorithm.ARGON2ID },
            {"argon2", PasswordHashAlgorithm.ARGON2 },
            {"argon2d", PasswordHashAlgorithm.ARGON2D },
            {"argon2i", PasswordHashAlgorithm.ARGON2I },
        };
        public enum PasswordHashAlgorithm
        {
            ARGON2ID,
            ARGON2,
            ARGON2D,
            ARGON2I
        }

        public static string Base64(byte[] bytes) => Convert.ToBase64String(bytes);
        public static byte[] Base64Decode(string base64String) => Convert.FromBase64String(base64String);

        public static ValueTask<PasswordHashArguments> CreateHash(string password, int degreeOfParallelism = 16, int iterations = 16, int memorySize = 16
            , int hashLength = 16, PasswordHashAlgorithm hashAlgorithm = PasswordHashAlgorithm.ARGON2ID, byte[] salt = default, byte[] hash = default)
        {
            var arguments = PasswordHashArguments.Create(degreeOfParallelism, iterations, memorySize, hashLength,  hashAlgorithm, salt, hash);

            return arguments.GenerateHash(password);
        }

        public static ValueTask<byte[]> CreateSalt()
        {
            return ValueTask.FromResult(RandomNumberGenerator.GetBytes(16));
        }

        public static async ValueTask<byte[]> HashPassword(string password, byte[] salt, int degreeOfParallelism = 16, int iterations = 16, int memorySize = 16, int hashLength = 16)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = degreeOfParallelism,
                Iterations = iterations,
                MemorySize = memorySize
            };

            return await argon2.GetBytesAsync(hashLength);
        }

        public static async ValueTask<PasswordHashArguments> GenerateHash(this PasswordHashArguments arguments, char[] password)
        {
            var salt = arguments.Salt ?? await CreateSalt();
            var _password = string.Concat(password);
            var hash = await HashPassword(_password, salt, arguments.DegreeOfParallelism, arguments.Iterations, arguments.MemorySize, arguments.HashLength).ConfigureAwait(false);
            return PasswordHashArguments.WithHash(arguments,salt,hash);
        }

        public static async ValueTask<PasswordHashArguments> GenerateHash(this PasswordHashArguments arguments, string password)
        {
            var salt = arguments.Salt ?? await CreateSalt();
            var hash = await HashPassword(password, salt, arguments.DegreeOfParallelism, arguments.Iterations, arguments.MemorySize, arguments.HashLength).ConfigureAwait(false);
            return PasswordHashArguments.WithHash(arguments, salt, hash);
        }

        public static bool VerifyHash(this PasswordHashArguments arguments, string password)
        {
            return arguments.VerifyHashAsync(password).GetAwaiter().GetResult();
        }

        public static async Task<bool> VerifyHashAsync(this PasswordHashArguments arguments, string password)
        {
            var newHash = await GenerateHash(arguments, password);
            return arguments.Hash.SequenceEqual(newHash.Hash);
        }

        public static bool VerifyHash(string password, string hash)
        {
            return From(hash).VerifyHash(password);
        }

        public static Task<bool> VerifyHashAsync(string password, string hash)
        {
            return From(hash).VerifyHashAsync(password);
        }

        public static PasswordHashArguments From(string hash)
        {
            return PasswordHashArguments.From(hash);
        }

    }
}
