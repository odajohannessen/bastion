using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Bastion.Core.Domain.Encryption.Services;

public class StorageService
{
    async Task<bool> StoreSecret(UserSecret userSecret)
    {
        // Check input UserSecret
        // TODO: Check other values? Valid timestamp etc?
        if (userSecret == null) 
        {
            return false;
        }

        // Secret 
        string jsonData = SecretStorageFormat(userSecret);

        // Key
        byte[] key = userSecret.Key;

        // TODO: Method for storing secret 
        
        // TODO: Method for storing key

        return true; // TODO: Return id on success? 
    }

    // Stores the jsonData string in blob storage


    // Stores the key in key vault



    // Creates json string format for storage of secret in blob
    public static string SecretStorageFormat(UserSecret userSecret)
    {
        // Exclude key from jsonData which is to be stored
        UserSecretJsonFormat secretJsonFormat = new UserSecretJsonFormat(userSecret.Id, userSecret.Ciphertext, userSecret.Lifetime, userSecret.TimeStamp, userSecret.IV);
        string jsonData = JsonConvert.SerializeObject(secretJsonFormat);

        return jsonData;
    }
}