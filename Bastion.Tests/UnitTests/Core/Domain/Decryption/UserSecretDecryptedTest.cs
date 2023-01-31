using Bastion.Core.Domain.Decryption;
using System.Security.Cryptography;

namespace Bastion.Tests.Core.Domain.Decryption;

public class UserSecretDecryptedTest
{
    [Fact]
    public void CreateUserSecretDecrypted()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string plaintext = "test";

        // Act
        UserSecretDecrypted userSecretDecrypted = new UserSecretDecrypted(id, plaintext);

        // Assert
        Assert.NotNull(userSecretDecrypted);
        Assert.Equal(id, userSecretDecrypted.Id);
        Assert.Equal(plaintext, userSecretDecrypted.Plaintext);
    }
}