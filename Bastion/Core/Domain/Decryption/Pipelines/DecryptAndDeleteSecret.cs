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
            
            try
            {
                // Get secret and key from storage
                // Key vault
                string keyVaultName = "kvbastion";
                string secretName = "userAssignedClientId";
                var uriKV = $"https://{keyVaultName}.vault.azure.net/";

                // Storage container
                string StorageContainerName = "secrets-test";
                string StorageAccountName = "sabastion";
                string blobName = $"{request.Id}.json";
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

                // Get blob
                BlobClient client = new BlobClient(new Uri(uriSA), credentials);
                MemoryStream ms = new MemoryStream();
                await client.DownloadToAsync(ms);
                string jsonData = Encoding.ASCII.GetString(ms.ToArray());

                // Convert to userSecret format
                userSecret = JsonConvert.DeserializeObject<UserSecretJsonFormat>(jsonData);
                if (userSecret == null)
                {
                    throw new Exception("Error deserializing");
                }
                ciphertext = Convert.FromBase64String(userSecret.Ciphertext);
  
                // Get key from key vault
                SecretClient secretClientKey = new SecretClient(new Uri(uriKV), new DefaultAzureCredential());
                Response<KeyVaultSecret> secretKey = secretClient.GetSecret(request.Id);
                var name = secretKey.Value.Value;
                var valueString = secretKey.Value.Value.ToString();
                secretKeyValue = Convert.FromBase64String(secretKey.Value.Value.ToString());
                //secretKeyValue = Encoding.UTF8.GetString(base64EncodedBytes);

                //secretKeyValue = System.Text.Encoding.Default.GetBytes(secretKey.Value.Value);
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message, ex);
            }

            // Decrypt secret
            try
            {
                plaintext = await DecryptionService.DecryptSecret(ciphertext, secretKeyValue , userSecret.IV); 

            }
            catch (Exception)
            {
                throw new Exception("Error decrypting secret");
            }

            // Delete secret and key
            try
            {
                success = await DeletionService.DeleteSecret(request.Id);
            }
            catch (Exception)
            {
                throw new Exception("Error deleting secret");
            }

            return new Response(success, plaintext);

        }
    }
}
