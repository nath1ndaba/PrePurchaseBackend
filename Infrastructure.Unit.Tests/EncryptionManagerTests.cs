using Xunit;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Infrastructure.Unit.Tests
{
    public class EncryptionManagerTests
    {
        private readonly ITestOutputHelper Output;
        public EncryptionManagerTests(ITestOutputHelper outputHelper)
        {
            Output = outputHelper;
        }
        

        [Theory]
        [InlineData("Some test data")]
        public async Task Encrypt_Should_Encrypt_Message_Returns_Key_Iv_encryptedMessage_Test(string message)
        {
            EncryptionManager manager = new();

            var (key, iv, encryptedMessage) = await manager.Encrypt(message);
            Output.WriteLine($"key:{key}, iv:{iv}\nencryptedMessage:{encryptedMessage}");
            Assert.NotEqual(message,encryptedMessage);
        }

        [Theory]
        [InlineData("2DSq5TcxrSUYbWvmDX0oqNlVhDF8X/38+1gWiaUF6No=", "X2CyGpHgRGyZgqp2D1FiVA==", "deLh+sFxBzoj29Y8qL1FsQ==", "Some test data")]
        public async Task Decrypt_Should_Decrypt_Encrypted_Message_Returns_Descrypted_Text_Test(string key, string iv, string encryptedMessage, string message)
        {
            EncryptionManager manager = new();

            var decryptedMessage = await manager.Decrypt(key, iv, encryptedMessage);
            Output.WriteLine($"decryptedMessage:{decryptedMessage}");
            Assert.Equal(message, decryptedMessage);
        }

    }
}