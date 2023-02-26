using Bastion.Core.Domain.Encryption;
using Bastion.Core.Domain.Encryption.Services;
using Bastion.Core.Domain.Decryption.Services;
using Bastion.Managers;

namespace Bastion.Tests.UnitTests.EncryptionDecryption;

public class EncryptionDecryptionTests
{
    [Fact]
    public void RoundtripTest()
    {
        // Arrange
        LoggingManager logging = new LoggingManager("APPLICATIONINSIGHTS_CONNECTION_STRING");
        string plaintext = "test";
        EncryptionService encryptionService = new EncryptionService(logging);
        DecryptionService decryptionService = new DecryptionService(logging);

        // Act
        var encryptionResponse = encryptionService.EncryptSecret(plaintext);
        var responseDecryption = decryptionService.DecryptSecret(encryptionResponse.Result.Item1, encryptionResponse.Result.Item2, encryptionResponse.Result.Item3);

        // Assert
        Assert.Equal(plaintext, responseDecryption.Result);

    }
}