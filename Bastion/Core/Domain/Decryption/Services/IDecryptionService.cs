namespace Bastion.Core.Domain.Decryption.Services;

public interface IDecryptionService
{
    // TODO: Encrypt secret
    // TODO: Store in Azure storage
    // TODO: Input? DTO object? userInput object? 
    // TODO: Return Bool? 
    // TODO: Should it be a task? 

    Task<string> DecryptSecret(byte[] ciphertext);
}
