using Bastion.Core.Domain.Encryption;

namespace Bastion.Core.Domain.Encryption.Services;

public interface IStorageService
{
    Task<bool> StoreSecret(UserSecret userSecret); // TODO: Return id on success? 
}
