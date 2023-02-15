namespace Bastion.Core.Domain.Decryption.Services;

public interface IDecryptionService
{
    Task<string> DecryptSecret(byte[] ciphertextBytes, byte[] key, byte[] IV);
}
