using Bastion.Core.Domain.UserInputSecret;
using System.Security.Cryptography;

namespace Bastion.Tests.Core.Domain.UserInputSecret;

public class UserInputTest
{
    [Fact]
    public void CreateUserSecret()
    {
        // Arrange
        string plaintext = "test";
        int lifetime = 1;

        // Act
        UserInput userInput = new UserInput(plaintext, lifetime);

        // Assert
        Assert.NotNull(userInput);
        Assert.Equal(plaintext, userInput.Plaintext); 
    }
}