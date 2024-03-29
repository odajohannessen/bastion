﻿using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using System.Text;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using Azure;
using Bastion.Helpers;
using Bastion.Managers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Graph;
using System.IO;

namespace Bastion.Core.Domain.Encryption.Services;

public class StorageService : IStorageService
{
    public LoggingManager logging;

    public StorageService(LoggingManager loggingManager)
    {
        logging = loggingManager;
    }

    public async Task<(bool, string)> StoreSecret(UserSecret userSecret)
    {
        // Check input UserSecret
        if (userSecret == null)
        {
            logging.LogException($"UserSecret input is null, storing failed");
            return (false, "");
        }

        // Secret format for storage
        string secretJsonFormat = SecretStorageFormat(userSecret);

        var successBlob = await StoreSecretInBlobStorage(secretJsonFormat, userSecret);

        if (!successBlob)
        {
            logging.LogException($"Storing of blob failed. ID: '{userSecret.Id}'.");
            return (false, "");
        }

        var successKey = await StorKeyInKeyVault(userSecret);

        if (!successKey)
        {
            logging.LogException($"Storing of key failed. ID: '{userSecret.Id}'.");
            return (false, "");
        }

        // Return id on success for url generation
        string id = userSecret.Id.ToString();

        return (true, id);
    }

    // Stores the jsonData string in blob storage and adds metadata of receivers to the blob
    private async Task<bool> StoreSecretInBlobStorage(string secretJsonFormat, UserSecret userSecret)
    {
        if (secretJsonFormat == null || userSecret == null) 
        {
            logging.LogException("Secret is empty.");
            throw new Exception("Secret cannot be empty");
        }

        // Storage container
        string StorageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
        string StorageContainerName = Environment.GetEnvironmentVariable("StorageContainerName");
        string blobName = userSecret.Id.ToString() + "--" + userSecret.ExpireTimeStamp.ToString("yyyy-MM-ddTHH:mm:ssK") + ".json";
        string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

        var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
        try
        {
            // Upload to blob
            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
 
            // Put request here as a work around for the reoccurring header issue for the web app
            TokenRequestContext requestContext = new TokenRequestContext(new[] { "https://storage.azure.com/.default" });
            AccessToken token = await credentials.GetTokenAsync(requestContext);
            string accessToken = token.Token;

            byte[] secretByteArray = Encoding.UTF8.GetBytes(secretJsonFormat);
            using (MemoryStream ms = new MemoryStream(secretByteArray))
            {
                //var uploadResponse = await client.UploadAsync(ms);
                HttpRequestMessage request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(uriSA),
                    Content = new StreamContent(ms)
                };
                request.Headers.Add("x-ms-version", "2020-04-08"); // Solves the issue
                request.Headers.Add("x-ms-blob-type", "BlockBlob");
                request.Headers.Add("Authorization", "bearer " + accessToken);

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    logging.LogException($"Error uploading secret to blob. ID: '{userSecret.Id}'. ");
                    return false;
                }
            }

            // Update receivers in metadata if receivers array in user secret is not null
            if (userSecret.OIDReceiver != null) 
            {
                IDictionary<string, string> receivers = new Dictionary<string, string>();
                foreach (string receiver in userSecret.OIDReceiver) 
                {
                    string receiverKey = RemoveHyphensFromGuid(receiver);
                    receivers.Add(receiverKey, "false"); // False - have not yet viewed the secret
                }
                Response<BlobInfo> response = client.SetMetadata(receivers);
                var responseDetails = response.GetRawResponse();
                if (responseDetails.Status == 200)
                {
                    logging.LogEvent($"Metadata successfully uploaded to secret with ID: '{userSecret.Id}'.");
                }
                else
                {
                    logging.LogException($"Error uploading metadata to secret with ID: '{userSecret.Id}'");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            logging.LogException($"Error uploading secret to blob storage: '{ex.Message}'. ID: '{userSecret.Id}'");
            return false;
        }

        return true;
    }

    // Stores the key in key vault
    private async Task<bool> StorKeyInKeyVault(UserSecret userSecret)
    {
        if (userSecret == null || userSecret.Key == null)
        {
            logging.LogException("User secret or key cannot be empty");
            throw new Exception("User secret or key cannot be empty");
        }

        SecretClientOptions options = new SecretClientOptions()
        {
            Retry =
            {
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
        };

        string keyVaultName = Environment.GetEnvironmentVariable("KeyVaultName");
        string uri = $"https://{keyVaultName}.vault.azure.net";
        string keyName = userSecret.Id.ToString(); // Key vault naming convention does not allow datetime format in string, only using id here
        string keyValue = Convert.ToBase64String(userSecret.Key);

        try
        {
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
            SecretClient client = new SecretClient(new Uri(uri), credentials, options);
            await client.SetSecretAsync(keyName, keyValue);
        }
        catch (Exception ex)
        {
            logging.LogException($"Error uploading key to key vault: '{ex.Message}'. ID: '{userSecret.Id}'");
            return false;
        }

        return true;
    }


    // Creates json string format for storage of secret in blob
    public static string SecretStorageFormat(UserSecret userSecret)
    {
        // Exclude key from jsonData which is to be stored
        UserSecretJsonFormat secretJsonFormat = new UserSecretJsonFormat(userSecret.Id, userSecret.Ciphertext, userSecret.Lifetime, userSecret.TimeStamp, userSecret.IV, userSecret.OIDSender);
        string jsonData = JsonConvert.SerializeObject(secretJsonFormat);

        return jsonData;
    }

    // Helper function to remove the hyphens "-" from the Guid of the OID, as these are not accepted in the key naming convention for metadata
    public static string RemoveHyphensFromGuid(string guid)
    {
        String chars = "[" + String.Concat('-') + "]";
        string guidStripped = Regex.Replace(guid, chars, string.Empty);
        return guidStripped;
    }
}