namespace Bastion.Core.Domain.Encryption.Services;

public interface IEncryptionService
{
    // TODO: Store in Azure storage
    // TODO: Return Bool? For testing: encrypted byte array, key and IV
    // TODO: Should it be a task? 

    Task<(byte[], byte[], byte[])> EncryptSecret(string plaintext);
}
