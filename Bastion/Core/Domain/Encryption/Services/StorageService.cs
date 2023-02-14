using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using System.Text;
using Microsoft.Azure.KeyVault;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using Azure;
using Bastion.Helpers;

namespace Bastion.Core.Domain.Encryption.Services;

public class StorageService : IStorageService
{
    public async Task<(bool, string)> StoreSecret(UserSecret userSecret)
    {
        // Check input UserSecret
        if (userSecret == null)
        {
            return (false, "");
        }

        // Secret format for storage
        string secretJsonFormat = SecretStorageFormat(userSecret);

        var successBlob = await StoreSecretInBlobStorage(secretJsonFormat, userSecret);

        if (!successBlob)
        {
            return (false, "");
        }

        var successKey = await StorKeyInKeyVault(userSecret);

        if (!successKey)
        {
            return (false, "");
        }

        // Return id on success for url generation
        string id = userSecret.Id.ToString();

        return (true, id);
    }

    // Stores the jsonData string in blob storage
    private async Task<bool> StoreSecretInBlobStorage(string secretJsonFormat, UserSecret userSecret)
    {
        if (secretJsonFormat == null || userSecret == null) 
        {
            throw new Exception("Secret cannot be empty");
        }

        // Storage container
        string StorageContainerName = "secrets-test";
        string StorageAccountName = "sabastion";
        string blobName = userSecret.Id.ToString() + "--" + userSecret.TimeStamp.ToString("yyyy-MM-ddTHH:mm:ssK") + ".json";
        string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

        var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
        try
        {
            // Upload to blob
            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
            byte[] secretByteArray = Encoding.UTF8.GetBytes(secretJsonFormat);
            MemoryStream ms = new MemoryStream(secretByteArray);
            await client.UploadAsync(ms);
        }
        catch (Exception)
        {
            return false;
            throw new Exception("Error uploading secret");
        }

        return true;
    }

    // Stores the key in key vault
    private async Task<bool> StorKeyInKeyVault(UserSecret userSecret)
    {
        if (userSecret == null || userSecret.Key == null)
        {
            throw new Exception("User secret or key cannot be empty");
        }

        SecretClientOptions options = new SecretClientOptions()
        {
            Retry =
            {
                Delay= TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
        };

        string keyVaultName = "kvbastion";
        string uri = $"https://{keyVaultName}.vault.azure.net";
        string keyName = userSecret.Id.ToString(); // Key vault naming convention does not allow datetime format in string, only using id here
        string keyValue = Convert.ToBase64String(userSecret.Key);

        try
        {
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
            SecretClient client = new SecretClient(new Uri(uri), credentials, options);
            await client.SetSecretAsync(keyName, keyValue);
        }
        catch (Exception)
        {
            return false;
            throw new Exception("Error uploading secret key");
        }

        return true;
    }


    // Creates json string format for storage of secret in blob
    public static string SecretStorageFormat(UserSecret userSecret)
    {
        // Exclude key from jsonData which is to be stored
        UserSecretJsonFormat secretJsonFormat = new UserSecretJsonFormat(userSecret.Id, userSecret.Ciphertext, userSecret.Lifetime, userSecret.TimeStamp, userSecret.IV);
        string jsonData = JsonConvert.SerializeObject(secretJsonFormat);

        return jsonData;
    }
}