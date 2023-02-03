using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using System.Text;

namespace Bastion.Core.Domain.Encryption.Services;

public class StorageService : IStorageService
{
    public async Task<bool> StoreSecret(UserSecret userSecret)
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
        var successBlob = await StoreSecretInBlobStorage(jsonData, userSecret.Id);

        if (!successBlob)
        {
            return false;
        }

        // TODO: Method for storing key
        // if (successKey)


        return true; // TODO: Return id on success? 
    }

    // Stores the jsonData string in blob storage
    private async Task<bool> StoreSecretInBlobStorage(string secretJsonFormat, Guid id)
    {
        if (secretJsonFormat == null) 
        {
            throw new Exception("Secret cannot be empty");
        }

        // Connection string (will be replaced by MI) and container name 
        var StorageAccountConnectionString = "<add here>";
        var StorageContainerName = "secrets-test";

        // File name
        string fileName = $"{id}.json";

        try
        {
            var blobServiceClient = new BlobServiceClient(StorageAccountConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(StorageContainerName);
            if (!await containerClient.ExistsAsync())
                await containerClient.CreateAsync();

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            byte[] secretByteArray = Encoding.UTF8.GetBytes(secretJsonFormat);
            MemoryStream ms = new MemoryStream(secretByteArray);
            await blobClient.UploadAsync(ms, true);

        }
        catch (Exception)
        {
            return false;
            throw new Exception("Error uploading secret");
        }

        return true;
    }

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