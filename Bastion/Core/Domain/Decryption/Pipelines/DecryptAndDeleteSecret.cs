﻿using MediatR;
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
            logging.LogEvent("A request to access a secret by an anonymous user has been received.");
            logging.LogEvent("Starting handling of request for decrypting and deleting secret.");

            if (request.Id == null)
            {
                throw new Exception("Id cannot be empty");
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
                logging.LogException("Secret does dot exist in key vault");
                return new Response(success, secretKey);
            }

            // Convert key 
            secretKeyValue = Convert.FromBase64String(secretKey);

            // Get list of blobs from storage container
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
                if (blobItem.Name.Contains(request.Id))
                {
                    blobName = blobItem.Name;
                    break;
                }
            }

            // Get blob
            string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";
            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
            MemoryStream ms = new MemoryStream();
            try
            {
                await client.DownloadToAsync(ms);
            }
            catch
            {
                logging.LogException("Secret does dot exist in blob storage");
                return new Response(success, "Blob not found");
            }
            string jsonData = Encoding.ASCII.GetString(ms.ToArray());

            // Convert to userSecret format
            userSecret = JsonConvert.DeserializeObject<UserSecretJsonFormat>(jsonData);
            if (userSecret == null)
            {
                throw new Exception("Error deserializing");
            }
            ciphertext = Convert.FromBase64String(userSecret.Ciphertext);

            // Decrypt secret
            plaintext = await DecryptionService.DecryptSecret(ciphertext, secretKeyValue, userSecret.IV); 

            // Delete secret and key
            success = await DeletionService.DeleteSecret(request.Id, blobName);

            if (success) 
            {
                logging.LogEvent("Secret succesfully accessed by anonymous user and deleted from storage.");
            }

            return new Response(success, plaintext);

        }
    }
}
