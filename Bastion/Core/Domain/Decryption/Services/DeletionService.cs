﻿using Newtonsoft.Json;
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

namespace Bastion.Core.Domain.Decryption.Services;

public class DeletionService : IDeletionService
{
    public async Task<bool> DeleteSecret(string id)
    {
        if (id == "")
        {
            return false;
        }

        // Delete secret from blob storage
        var successBlob = await DeleteSecretFromBlobStorage(id);
        if (!successBlob)
        {
            return false;
        }

        // Delete key from key vault
        var successKey = await DeleteKeyFromKeyVault(id);
        if (!successKey)
        {
            return false;
        }

        return true; 
    }

    // Delete secret from blob storage
    private async Task<bool> DeleteSecretFromBlobStorage(string id)
    {
        if (id == null) 
        {
            throw new Exception("Id cannot be empty");
        }

        // Storage container
        string StorageContainerName = "secrets-test";
        string StorageAccountName = "sabastion";
        string blobName = $"{id}.json";
        string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

        var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
        try
        {
            // Delete blob
            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
            await client.DeleteAsync();
        }
        catch (Exception)
        {
            return false;
            throw new Exception("Error uploading secret");
        }

        return true;
    }

    // Delete key from key vault
    private async Task<bool> DeleteKeyFromKeyVault(string id)
    {
        if (id == null)
        {
            throw new Exception("Id cannot be empty");
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

        try
        {
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
            SecretClient client = new SecretClient(new Uri(uri), credentials, options);
            await client.StartDeleteSecretAsync(id);
        }
        catch (Exception)
        {
            return false;
            throw new Exception("Error uploading secret key");
        }

        return true;
    }

}