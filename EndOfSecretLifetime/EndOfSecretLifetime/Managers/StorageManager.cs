using Azure.Core;
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

        // TODO: Add func app to MI 

        // Retrieve a list of the secrets currently in storage
        List<string> secretList = GetBlobNames(storageContainerName);

        // Extract expiration time stamp from file name and check if they are expired
        List<string> secretExpiredList = CheckExpireTimeStamp(secretList);

        // Delete the expired secrets from blob storage and key vault


        // What should the timer trigger do? 

        // 2. Innebygd sjekk om noen har levd mer enn 24 t? 
        // Sett grense på 24 t
        // List -> dict(id, dato) fra navn
        // ISO8601 for date

        return true;
    }


    // Get list of blobs in a storage container
    public static List<string> GetBlobNames(string storageContainerName)
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
        catch 
        {
            throw new Exception("Error retrieving list of blobs");
        }
    }

    // Input list of blobs in storage container, return list of blobs of secrets which are expired
    public static List<string> CheckExpireTimeStamp(List<string> secretList)
    { 
        List<string> secretExpiredList = new List<string>();

        foreach (string secretName in secretList) 
        {
            // Extract expire time stamp from file name
            try
            {
                int from = secretName.IndexOf("--") + 2;
                int to = secretName.IndexOf(".");
                string expireTimeStampString = secretName.Substring(from, to - from);
                DateTime expireTimeStamp = DateTime.Parse(expireTimeStampString);

                // If secret is expired, add it to the list
                if (expireTimeStamp < DateTime.Now)
                {
                    secretExpiredList.Add(secretName);
                }
            }
            catch 
            { 
                throw new Exception("Error extracting and checking for expired blobs");
            }
        }

        return secretExpiredList;
    }
}
