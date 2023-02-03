namespace Bastion.Core.Domain.Encryption.Services;

public interface IEncryptionService
{
    Task<(byte[], byte[], byte[])> EncryptSecret(string plaintext);
}
