using System;
using System.Threading.Tasks;
using static Infrastructure.PasswordHelper;

namespace Infrastructure
{
    public struct PasswordHashArguments
    {
        /// <summary>
        /// Argon2 hash algorithm to use
        /// </summary>
        public PasswordHashAlgorithm HashAlgorithm { get; private set; }
        //private char[] Password { get; set; }
        internal byte[] Salt { get;}

        /// <summary>
        /// Number of virtual cores to use for parallelization
        /// </summary>
        public int DegreeOfParallelism { get; set; }
        public int Iterations { get; set; }
        /// <summary>
        /// How much memory to consume when hashing in Kilobytes (KB)
        /// </summary>
        public int MemorySize { get; set; }
        /// <summary>
        /// The length of the hash generated, a number from 4 to 100
        /// </summary>
        public int HashLength { get; set; }
        internal byte[] Hash { get;}

        private PasswordHashArguments(PasswordHashAlgorithm hashAlgorithm = PasswordHashAlgorithm.ARGON2ID
            , byte[] salt = default, int degreeOfParallelism = 1, int iterations = 4, int memorySize = 16
            , int hashLength = 16, byte[] hash = default)
        {
            HashAlgorithm = hashAlgorithm;
            //Password = password;
            Salt = salt;
            DegreeOfParallelism = degreeOfParallelism;
            Iterations = iterations;
            MemorySize = memorySize;
            HashLength = hashLength;
            Hash = hash;

        }

        internal static PasswordHashArguments From(string hash)
        {
            return hash;
        }
        
        internal static PasswordHashArguments Create(
            int degreeOfParallelism, int iterations, int memorySize
            , int hashLength, PasswordHashAlgorithm hashAlgorithm = PasswordHashAlgorithm.ARGON2ID, byte[] salt = default, byte[] hash = default)
        {
            return new(hashAlgorithm, salt, degreeOfParallelism, iterations, memorySize, hashLength, hash);
        }

        internal static PasswordHashArguments WithHash(PasswordHashArguments arguments, byte[] salt, byte[] hash)
        {
            return new(salt: salt, hash: hash)
            {
                HashAlgorithm = arguments.HashAlgorithm,
                DegreeOfParallelism = arguments.DegreeOfParallelism,
                Iterations = arguments.Iterations,
                MemorySize = arguments.MemorySize,
                HashLength = arguments.HashLength
            };
        }


        public override string ToString()
        {
            var hash = $"${HashAlgorithm.ToString().ToLower()}$m={MemorySize},t={Iterations},p={DegreeOfParallelism}";
            hash += $"${Base64(Salt)}${Base64(Hash)}";
            return hash;
        }

        public static implicit operator string(PasswordHashArguments arguments)
            => arguments.ToString();

        /// <summary>
        /// Uses formated hash string and returns a <see cref="PasswordHashArguments"/>.
        /// hash format: $algorithm$memorySize=integer,iterations=integer,degreeOfParallelism=integer$salt$hash
        /// </summary>
        /// <param name="hashString"></param>

        public static implicit operator PasswordHashArguments(string hashString)
        {
            if (!hashString.StartsWith("$"))
                throw new ArgumentException($"The hash is of invalid format, {nameof(hashString)}");

            hashString = hashString[1..]; // get the remaing characters from index 1 to the end
            var args = hashString.Split("$");

            if (args.Length != 4)
                throw new ArgumentException($"The hash is of invalid format, {nameof(hashString)}");

            var algorithm = PasswordHasingAlgorithms[args[0]];
            var salt = Base64Decode(args[2]);
            var hash = Base64Decode(args[3]);

            var paramters = args[1].Split(",");

            if (paramters.Length != 3)
                throw new ArgumentException($"The hash is of invalid format, {nameof(hashString)}");

            var memory = int.Parse(paramters[0].Split("=")[1]);
            var iterations = int.Parse(paramters[1].Split("=")[1]);
            var degreeOfParallelism = int.Parse(paramters[2].Split("=")[1]);

            return new (salt: salt, hash:hash)
            {
                HashAlgorithm = algorithm,
                DegreeOfParallelism = degreeOfParallelism,
                Iterations = iterations,
                MemorySize = memory,
                HashLength = hash.Length

            };

        }
    }
}
