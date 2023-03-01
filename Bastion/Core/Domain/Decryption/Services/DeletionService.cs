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
using Bastion.Core.Domain.Encryption;
using Bastion.Helpers;
using Bastion.Managers;

namespace Bastion.Core.Domain.Decryption.Services;

public class DeletionService : IDeletionService
{
    public LoggingManager logging;

    public DeletionService(LoggingManager loggingManager)
    {
        logging = loggingManager;
    }

    public async Task<bool> DeleteSecret(string id, string blobName)
    {
        if (id == null || blobName == null)
        {
            logging.LogException("Id or blob name is null.");
            return false;
        }

        // Delete secret from blob storage
        var successBlob = await DeleteSecretFromBlobStorage(blobName);
        if (!successBlob)
        {
            logging.LogException($"Deletion of blob failed. ID: '{id}'.");
            return false;
        }

        // Delete key from key vault
        var successKey = await DeleteKeyFromKeyVault(id);
        if (!successKey)
        {
            logging.LogException($"Deletion of key failed. ID: '{id}'.");
            return false;
        }

        return true; 
    }

    // Delete secret from blob storage
    private async Task<bool> DeleteSecretFromBlobStorage(string blobName)
    {
        // Storage container
        string StorageContainerName = "secrets-test";
        string StorageAccountName = "sabastion";
        string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

        var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
        try
        {
            // Delete blob
            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
            await client.DeleteAsync();
        }
        catch (Exception ex)
        {
            logging.LogException($"Error deleting blob: '{ex.Message}'. Blob name: '{blobName}'.");
            return false;
        }

        return true;
    }

    // Delete key from key vault
    private async Task<bool> DeleteKeyFromKeyVault(string id)
    {
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

        string keyVaultName = "kvbastion-secrets"; 
        string uri = $"https://{keyVaultName}.vault.azure.net";

        try
        {
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
            SecretClient client = new SecretClient(new Uri(uri), credentials, options);
            await client.StartDeleteSecretAsync(id);
        }
        catch (Exception ex)
        {
            logging.LogException($"Error deleting key: '{ex.Message}'. ID: '{id}'.");
            return false;
        }

        return true;
    }

}