using Bastion.Core.Domain.Encryption;

namespace Bastion.Core.Domain.Encryption.Services;

public interface IStorageService
{
    Task<(bool, string)> StoreSecret(UserSecret userSecret);
}
