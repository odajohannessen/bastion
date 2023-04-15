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
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;

namespace Bastion.Core.Domain.Decryption.Pipelines;

public class DecryptAndDeleteSecret
{
    public record Request(string Id, string OIDUser="") : IRequest<Response>;
    public record Response(bool success, string Plaintext, string OIDSender);

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
            if (!request.OIDUser.IsNullOrEmpty())
            {
                logging.LogEvent($"Starting handling of request for user with OID '{request.OIDUser}' for accessing secret. ID: '{request.Id}'.");
            }
            else
            {
                logging.LogEvent($"Starting handling of anonymous request accessing secret. ID: '{request.Id}'.");
            }

            if (request.Id == null)
            {
                logging.LogException($"Id cannot be null");
                throw new Exception("Id cannot be null"); // throw or return here? Is this even necessary?
            }

            bool success = false;

            // Check if given id is in a GUID format
            var result = Regex.Replace(request.Id, @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$","'$0'");
            if (string.Equals(request.Id, result))
            {
                logging.LogTrace($"Secret entered not on GUID format, and not a valid page.");
                return new Response(success, "Invalid GUID", "");
            }
            else
            {
                logging.LogTrace($"Guid secret format registered.");
            }

            // Get blobname from storage container
            string blobName = GetBlobNameFromStorageContainer(request.Id);
            if (blobName == "")
            {
                logging.LogException("Secret does not exist in storage container.");
                return new Response(success, "Secret not found", "");
            }
            // Check if secret has receivers, returns true if metadata count is not 0, false if count is higher
            bool hasReceivers = CheckIfSecretHasReceivers(blobName);
            // If the secret has receivers, but the user is not logged in, return response requiring login
            if (hasReceivers && request.OIDUser == "") 
            {
                logging.LogEvent($"Anonymous user trying to access a secret with defined receiver(s). Id: '{request.Id}'.");
                return new Response(success, "Login required", "");
            }

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
                return new Response(success, secretKey, "");
            }

            // Convert key 
            secretKeyValue = Convert.FromBase64String(secretKey);

            // Get json data from blob
            string jsonData = await GetJsonDataFromBlob(blobName);
            if (jsonData == "Blob not found")
            {
                logging.LogException("Error retreiving secret from storage container.");
                return new Response(success, "Secret not found", "");
            }

            // Convert to userSecret format
            userSecret = JsonConvert.DeserializeObject<UserSecretJsonFormat>(jsonData);
            if (userSecret == null)
            {
                throw new Exception("Error deserializing");
            }
            ciphertext = Convert.FromBase64String(userSecret.Ciphertext);

            // Confirm the user who has requested to see the secret is the intended receiver
            if (!request.OIDUser.IsNullOrEmpty())
            {
                //bool successHash = HashingHelper.VerifyHash(request.OIDReceiver, userSecret.OIDReceiver);
                //if (!successHash)

                // Check if the OID of the current user is one of the intended receivers or not, or if they have already viewed the secret
                // Returns true if the current user is an intended receiver who has not viewed the secret yet, false if not
                bool receiverStatus = CheckSecretReceiver(blobName, request.OIDUser);
                if (!receiverStatus)
                {
                    return new Response(success, "You are not an intended recipient, or has already viewed this secret once.", "");
                }
            }

            // Decrypt secret
            plaintext = await DecryptionService.DecryptSecret(ciphertext, secretKeyValue, userSecret.IV);
            logging.LogEvent($"Secret successfully decrypted. ID: {request.Id}");

            // Delete secret and key if all receivers have viewed it or if there are no receivers
            bool viewerStatus = CheckSecretViewerStatus(blobName);
            if (viewerStatus)
            {
                success = await DeletionService.DeleteSecret(request.Id, blobName);
            }
            else
            { 
                success = true;
            }

            if (success) 
            {
                if (request.OIDUser.IsNullOrEmpty())
                {
                    logging.LogEvent($"Secret succesfully accessed by anonymous user and deleted from storage. ID: '{request.Id}'.");
                }
                else
                {
                    if (viewerStatus)
                        logging.LogEvent($"Secret succesfully accessed by user with OID ${request.OIDUser} and deleted from storage. ID: '{request.Id}'.");
                    else
                        logging.LogEvent($"Secret succesfully accessed by user with OID ${request.OIDUser}. ID: '{request.Id}'.");
                }
            }

            return new Response(success, plaintext, userSecret.OIDSender);
        }
        public async Task<string> GetJsonDataFromBlob(string blobName)
        {
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();

            string StorageContainerName = "secrets-test";
            string StorageAccountName = "sabastionsecrets";
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
            string StorageAccountName = "sabastionsecrets";
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

        // Return true if the secret has intended receivers, false if not
        public static bool CheckIfSecretHasReceivers(string blobName)
        {
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();

            string StorageContainerName = "secrets-test";
            string StorageAccountName = "sabastionsecrets";
            string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
            BlobProperties properties = client.GetProperties();

            if (properties.Metadata.Count != 0)
            {
                return true;
            }

            return false;
        }


        // Return true if OIDReceiver has not yet viewed the secret, false if they have
        // Return true if there are no receivers
        // Will also return false if the receiver is not found among the receivers
        public static bool CheckSecretReceiver(string blobName, string OIDUser)
        {   
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();

            string StorageContainerName = "secrets-test";
            string StorageAccountName = "sabastionsecrets";
            string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
            BlobProperties properties = client.GetProperties();

            // There are no receivers of this secret
            if (properties.Metadata.Count == 0)
            {
                return true;
            }

            IDictionary<string, string> receiver = new Dictionary<string, string>();
            bool receiverFound = false;

            foreach (var metadataItem in properties.Metadata) 
            {
                string guid = AddHyphensToGuid(metadataItem.Key);
                // If the user's OID is present in the metadata, and the value is false, the user has not viewed the secret yet
                // If the user's OID is present in the metadata, and the value is true, the user has already viewed the secret once
                // If the OID is not in the metadata, they are not an intended recipient
                if (guid == OIDUser)
                {
                    // Has not yet viewed the secret
                    if (metadataItem.Value == "false")
                    {
                        receiverFound = true;
                        // Update metadata for the current receiver
                        receiver.Add(metadataItem.Key, "true");
                    }
                    else
                    {
                        receiver.Add(metadataItem.Key, "false");
                    }
                }
                else
                {
                    receiver.Add(metadataItem.Key, metadataItem.Value);
                }
            }

            if (receiverFound)
            {
                // Update the metadata (overwrites all existing metadata)
                client.SetMetadata(receiver);
                return true;
            }

            return false;
        }

        // Check if all receivers have opened and viewed the secret
        // Return true if they have or if there are no receivers, false if not
        public static bool CheckSecretViewerStatus(string blobName)
        {
            var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();

            string StorageContainerName = "secrets-test";
            string StorageAccountName = "sabastionsecrets";
            string uriSA = $"https://{StorageAccountName}.blob.core.windows.net/{StorageContainerName}/{blobName}";

            BlobClient client = new BlobClient(new Uri(uriSA), credentials);
            BlobProperties properties = client.GetProperties();

            // There are no receivers of this secret, it can be deleted after the first visit to the URL
            if (properties.Metadata.Count == 0)
            {
                return true;
            }

            foreach (var metadataItem in properties.Metadata)
            {
                // If the value is false, the user has not viewed the secret yet
                // If the value is true, the user has already viewed the secret once
                if (metadataItem.Value == "false")
                {
                    // Return false if at least one of the receivers have not viewed the secret yet
                    return false;
                }
            }

            return true;
        }

        // Helper function to add the hyphens "-" to the Guid of the OID key from metadata
        // This needs to be done because hyphens "-" are not accepted in the key naming convention for metadata
        public static string AddHyphensToGuid(string guidStripped)
        {
            // Guids are given in the format: 00000000-0000-0000-0000-000000000000 (Characters: 8 - 4 - 4 - 4 - 12)
            // The input key format is: 0000000000000000000000000000
            string subString1 = guidStripped.Substring(0, 8);
            string subString2 = guidStripped.Substring(8, 4);
            string subString3 = guidStripped.Substring(12, 4);
            string subString4 = guidStripped.Substring(16, 4);
            string subString5 = guidStripped.Substring(20, 12);

            string guid = subString1 + "-" + subString2 + "-" + subString3 + "-" + subString4 + "-" + subString5;

            return guid;
        }
    }
}
