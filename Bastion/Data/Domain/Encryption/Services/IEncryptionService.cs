namespace Bastion.Data.Domain.Encryption.Services;

public interface IEncryptionService
{
    // TODO: Encrypt secret
    // TODO: Store in Azure storage
    // TODO: Input? DTO object? userInput object? 
    // TODO: Return Bool? 
    // TODO: Should it be a task? 

    Task<bool> EncryptSecret(string plaintext);
}
