using Bastion.Core.Domain.Encryption;
using System.Security.Cryptography;

namespace Bastion.Tests.Core.Domain.Encryption;

public class UserSecretTest
{
    [Fact]
    public void CreateUserSecret()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string ciphertext = "test";
        int lifetime = 1;
        DateTime timestamp = DateTime.UtcNow;
        byte[] key;
        byte[] IV;
        using (Aes aes = Aes.Create()) 
        {
            key = aes.Key;
            IV = aes.IV;
        }

        // Act
        UserSecret userSecret = new UserSecret(id, ciphertext, lifetime, timestamp, key, IV);

        // Assert
        Assert.Equal(id, userSecret.Id);
        Assert.Equal(ciphertext, userSecret.Ciphertext);
        Assert.Equal(lifetime, userSecret.Lifetime);
        Assert.Equal(timestamp, userSecret.TimeStamp);
        Assert.Equal(key, userSecret.Key);
        Assert.Equal(IV, userSecret.IV);
    }
}