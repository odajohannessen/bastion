using Bastion.Core.Domain.Encryption;
using Bastion.Core.Domain.Encryption.Services;
using Bastion.Core.Domain.Decryption.Services;

namespace Bastion.Tests.UnitTests.EncryptionDecryption;

public class EncryptionDecryptionTests
{
    [Fact]
    public void RoundtripTest()
    {
        // Arrange
        string plaintext = "test";
        EncryptionService encryptionService = new EncryptionService();
        DecryptionService decryptionService = new DecryptionService();

        // Act
        var encryptionResponse = encryptionService.EncryptSecret(plaintext);
        var responseDecryption = decryptionService.DecryptSecret(encryptionResponse.Result.Item1, encryptionResponse.Result.Item2, encryptionResponse.Result.Item3);

        // Assert
        Assert.Equal(plaintext, responseDecryption.Result);

    }
}