using MediatR;
using Bastion.Core.Domain.Decryption;
using Bastion.Core.Domain.Decryption.Services;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using Azure.Storage.Blobs;
using Azure;
using Bastion.Core.Domain.Encryption;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography.Xml;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Bastion.Helpers;
using Bastion.Managers;
using Azure.Storage.Blobs.Models;
using System.Runtime.CompilerServices;

namespace Bastion.Core.Domain.Decryption.Pipelines;

public class DecryptAndDeleteSecret
{
    public record Request(string Id) : IRequest<Response>;
    public record Response(bool success, string Plaintext);

    public class Handler : IRequestHandler<Request, Response>
    {
        public IDecryptionService DecryptionService;
        public IDeletionService DeletionService;
        public LoggingManager logging;

        public Handler(IDecryptionService decryptionService, IDeletionService deletionService, LoggingManager loggingManager)
        {
            DecryptionService = decryptionService;
            DeletionService = deletionService;
            logging = loggingManager;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            logging.LogEvent($"Starting handling of anonymous request for decrypting and deleting secret. ID: '{request.Id}'.");

            if (request.Id == null)
            {
                logging.LogException($"Id cannot be null");
                throw new Exception("Id cannot be null");
            }

            bool success = false;
            string plaintext;
            byte[] ciphertext;
            byte[] secretKeyValue;
            UserSecretJsonFormat userSecret;

            // Get key from key vault
            string secretKey = GetSecretFromKeyVaultHelper.GetSecret(request.Id);

            // Check if id exists in key vault
            if (secretKey == "Secret not found")
            {
                logging.LogException("Secret does not exist in key vault.");
                return new Response(success, secretKey);
            }

            // Convert key 
            secretKeyValue = Convert.FromBase64String(secretKey);

            // Get blobname from storage container
            string blobName = GetBlobNameFromStorageContainer(request.Id);
            if (blobName == "")
            {
                logging.LogException("Secret does not exist in storage container.");
                return new Response(success, "Secret not found");
            }

            // Get json data from blob
            string jsonData = await GetJsonDataFromBlob(blobName);
            if (jsonData == "Blob not found")
            {
                logging.LogException("Error retreiving secret from storage container.");
                return new Response(success, "Secret not found");
            }

            // Convert to userSecret format
            userSecret = JsonConvert.DeserializeObject<UserSecretJsonFormat>(jsonData);
            if (userSecret == null)
            {
                throw new Exception("Error deserializing");
            }
            ciphertext = Convert.FromBase64String(userSecret.Ciphertext);

            // Decrypt secret
            plaintext = await DecryptionService.DecryptSecret(ciphertext, secretKeyValue, userSecret.IV);
            logging.LogEvent($"Secret successfully decrypted. ID: {request.Id}");

            // Delete secret and key
            success = await DeletionService.DeleteSecret(request.Id, blobName);

            if (success) 
            {
                logging.LogEvent($"Secret succesfully accessed by anonymous user and deleted from storage. ID: '{request.Id}'.");
            }

            return new Response(success, plaintext);
        }
        public async Task<string> GetJsonDataFromBlob(string blobName)
        {
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();

            string StorageContainerName = "secrets-test";
            string StorageAccountName = "sabastion";
            string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
            MemoryStream ms = new MemoryStream();
            try
            {
                await client.DownloadToAsync(ms);
            }
            catch
            {
                logging.LogException($"Secret does dot exist in blob storage. Blob name: '{blobName}'.");
                return "Blob not found";
            }

            string jsonData;
            try
            {
                jsonData = Encoding.ASCII.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                logging.LogException($"Error deserializing data in blob. Blob name: '{blobName}'.");
                throw new Exception(ex.Message);
            }

            return jsonData;
        }

        // Get list of blobs in a storage container and return the blob name matching the id
        public static string GetBlobNameFromStorageContainer(string id)
        {
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();

            string StorageContainerName = "secrets-test";
            string StorageAccountName = "sabastion";
            string uriContainer = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}";
            string blobName = "";

            BlobContainerClient containerClient = new BlobContainerClient(new Uri(uriContainer), credentials);
            var blobItems = containerClient.GetBlobs();

            foreach (BlobItem blobItem in blobItems)
            {
                // Check if file name contains the secret id
                if (blobItem.Name.Contains(id))
                {
                    blobName = blobItem.Name;
                    break;
                }
            }

            return blobName;
        }

    }
}
