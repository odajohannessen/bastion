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

namespace Bastion.Core.Domain.Decryption.Pipelines;

public class DecryptAndDeleteSecret
{
    public record Request(string Id) : IRequest<Response>;
    public record Response(bool success, string Plaintext);

    public class Handler : IRequestHandler<Request, Response>
    {
        public IDecryptionService DecryptionService;
        public IDeletionService DeletionService;

        public Handler(IDecryptionService decryptionService, IDeletionService deletionService)
        {
            DecryptionService = decryptionService;
            DeletionService = deletionService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
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
                return new Response(success, secretKey);
            }

            // Convert if it exists
            secretKeyValue = Convert.FromBase64String(secretKey);
            // secretKeyValue = Encoding.UTF8.GetString(base64EncodedBytes);
            //secretKeyValue = System.Text.Encoding.Default.GetBytes(secretKey.Value.Value);
                
            // Storage container
            string StorageContainerName = "secrets-test";
            string StorageAccountName = "sabastion";
            string blobName = $"{request.Id}.json";
            string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();

            // Get blob
            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
            MemoryStream ms = new MemoryStream();
            try
            {
                await client.DownloadToAsync(ms);
            }
            catch
            {
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
            plaintext = await DecryptionService.DecryptSecret(ciphertext, secretKeyValue , userSecret.IV); 

            // Delete secret and key
            success = await DeletionService.DeleteSecret(request.Id);

            return new Response(success, plaintext);

        }
    }
}
