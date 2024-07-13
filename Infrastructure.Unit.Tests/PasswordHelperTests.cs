using Xunit;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using System.Threading.Tasks;
using static Infrastructure.PasswordHelper;

namespace Infrastructure.Unit.Tests
{
    public class PasswordHelperTests
    {
        private readonly ITestOutputHelper Output;
        public PasswordHelperTests(ITestOutputHelper outputHelper)
        {
            Output = outputHelper;
        }

        [Theory]
        [InlineData("$argon2id$m=16,t=4,p=1$gM64oYFh9laIksFYCHPj4w==$04DwQs+AyKBjX/ArQWIFSA==", "password")]
        public async Task VerifyHashAsync_From_Password_and_Hash_Test(string hash, string password)
        {
            var result = await VerifyHashAsync(password, hash);
            Assert.True(result);
        }

        [Theory]
        [InlineData("$argon2id$m=16,t=4,p=1$gM64oYFh9laIksFYCHPj4w==$04DwQs+AyKBjX/ArQWIFSA==", "password")]
        public async Task GenerateHash_From_Password_and_Hash_Test(string hash, string password)
        {
            var newHash = await From(hash).GenerateHash(password);

            Assert.Equal(hash, newHash);

        }
    }
}