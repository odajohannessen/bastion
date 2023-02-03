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

        // Secret format for storage
        string jsonData = SecretStorageFormat(userSecret);

        var successBlob = await StoreSecretInBlobStorage(jsonData, userSecret.Id);

        if (!successBlob)
        {
            return false;
        }

        var successKey = await StorKeyInKeyVault(userSecret.Key, userSecret.Id);

        if (!successKey)
        {
            return false;
        }

        return true; // TODO: Return id on success? 
    }

    // Stores the jsonData string in blob storage
    private async Task<bool> StoreSecretInBlobStorage(string secretJsonFormat, Guid id)
    {
        if (secretJsonFormat == null) 
        {
            throw new Exception("Secret cannot be empty");
        }

        // Key vault
        string keyVaultName = "kvbastion";
        string secretName = "userAssignedClientId";
        var uriKV = $"https://{keyVaultName}.vault.azure.net/";
        
        // Storage container
        string StorageContainerName = "secrets-test";
        string StorageAccountName = "sabastion";
        string blobName = $"{id}.json";
        string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

        // Get User assigned client ID from key vault (through SA MI between web app and key vault)
        SecretClient secretClient = new SecretClient(new Uri(uriKV), new DefaultAzureCredential());
        Response<KeyVaultSecret> secret = secretClient.GetSecret(secretName);
        string userAssignedClientId = secret.Value.Value.ToString();

        var options = new DefaultAzureCredentialOptions
        {
            ExcludeEnvironmentCredential = true,
            ExcludeManagedIdentityCredential = false,
            ManagedIdentityClientId = userAssignedClientId,
        };
        var credentials = new DefaultAzureCredential();

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
    private async Task<bool> StorKeyInKeyVault(byte[] key, Guid id)
    {
        if (key == null)
        {
            throw new Exception("Key cannot be empty");
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

        string keyVaultName = "kvbastion"; // TODO: Add somewhere else? Will always be the same
        string uri = $"https://{keyVaultName}.vault.azure.net";
        string keyName = id.ToString();
        string keyValue = Encoding.UTF8.GetString(key);

        try
        {
            // TODO: Turn on MI for web app once launched in Azure
            SecretClient client = new SecretClient(new Uri(uri), new DefaultAzureCredential(), options);
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