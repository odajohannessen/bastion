using Bastion.Core.Domain.Encryption;

namespace Bastion.Core.Domain.Decryption.Services;

public interface IDeletionService
{
    Task<bool> DeleteSecret(string id, string blobName);
}
