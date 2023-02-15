﻿using Azure.Core;
using Azure.Storage.Blobs;
using Azure;
using Azure.Storage.Blobs.Models;
using EndOfSecretLifetime.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Azure.Security.KeyVault.Secrets;

namespace EndOfSecretLifetime.Managers;

public class StorageManager
{
    public LoggingManager logging;

    public StorageManager(LoggingManager loggingManager)
    {
        logging = loggingManager;
    }

    public async Task<bool> CheckExpirationAndDelete(string storageContainerName)
    {
        // Retrieve a list of the secrets currently in storage
        List<string> secretList = GetBlobNames(storageContainerName);
        
        if (secretList.Count == 0) 
        {
            logging.LogTrace("No secrets in storage.");
            return true;
        }

        // Extract expiration time stamp from file name and check if they are expired
        Dictionary<string, string> secretExpiredDict = CheckExpireTimeStamp(secretList);
        if (secretExpiredDict.Count == 0)
        {
            logging.LogTrace("No secrets have expired.");
            return true;
        }

        // Delete the expired secrets from blob storage and key vault
        foreach (KeyValuePair<string, string> secret in secretExpiredDict) 
        { 
            await DeleteBlob(storageContainerName, secret.Value);

            await DeleteKey(secret.Key);
        }

        logging.LogEvent("Expired secrets successfully deleted.");
        return true;
    }

    // Delete a blob from blob storage
    public async Task<bool> DeleteBlob(string storageContainerName, string blobname)
    {
        var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
        string StorageAccountName = "sabastion";
        string uri = $"https://{StorageAccountName}.blob.core.windows.net/{storageContainerName}/{blobname}";
        BlobClient blobClient = new BlobClient(new Uri(uri), credentials);

        try 
        {
            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception e) 
        {
            throw new Exception($"Error deleting blob: '{e.Message}'");
        }

        return true;
    }

    // Delete a key from key vault
    public async Task<bool> DeleteKey(string keyName)
    {
        string keyVaultName = "kvbastion";
        var uri = $"https://{keyVaultName}.vault.azure.net";
        var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
        SecretClient client = new SecretClient(new Uri(uri), credentials);

        try
        {
            await client.StartDeleteSecretAsync(keyName);
        }
        catch (Exception e)
        {
            throw new Exception($"Error deleting key: '{e.Message}'");
        }

        return true;
    }


    // Input list of blobs in storage container, return list of blobs of secrets which are expired
    public Dictionary<string, string> CheckExpireTimeStamp(List<string> secretList)
    {
        Dictionary<string, string> secretExpiredDict = new Dictionary<string, string>();

        foreach (string secretName in secretList)
        {
            // Extract expire time stamp from file name
            // ISO8601 format for datetime string
            try
            {
                int from = secretName.IndexOf("--") + 2;
                int to = secretName.IndexOf(".");
                string expireTimeStampString = secretName.Substring(from, to - from);
                DateTime expireTimeStamp = DateTime.Parse(expireTimeStampString);

                // If secret is expired, add it to the dict
                if (expireTimeStamp < DateTime.Now)
                {
                    // Extract id
                    int fromId = 0;
                    int toId = secretName.IndexOf("--");
                    string id = secretName.Substring(fromId, toId - fromId);

                    secretExpiredDict.Add(id, secretName);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error extracting and checking for expired blobs: '{e.Message}'");
            }
        }

        return secretExpiredDict;
    }


    // Get list of blobs in a storage container
    public List<string> GetBlobNames(string storageContainerName)
    {
        var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();

        string StorageAccountName = "sabastion";
        string uriContainer = $"https://{StorageAccountName}.blob.core.windows.net/{storageContainerName}";

        try
        {
            BlobContainerClient containerClient = new BlobContainerClient(new Uri(uriContainer), credentials);
            var blobItems = containerClient.GetBlobs();

            List<string> secretList = new List<string>();

            foreach (BlobItem blobItem in blobItems)
            {
                secretList.Add(blobItem.Name);
            }

            return secretList;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving list of blobs: '{e.Message}'");
        }
    }

}