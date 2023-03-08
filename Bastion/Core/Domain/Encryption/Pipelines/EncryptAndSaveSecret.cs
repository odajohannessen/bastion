using MediatR;
using Bastion.Core.Domain.Encryption;
using Bastion.Core.Domain.Encryption.Services;
using Bastion.Core.Domain.UserInputSecret.Dto;
using Bastion.Managers;
using System.Text;
using Bastion.Helpers;

namespace Bastion.Core.Domain.Encryption.Pipelines;

public class EncryptAndSaveSecret
{
    public record Request(UserInputDto userInputDto) : IRequest<Response>; 
    public record Response(byte[] CiphertextBytes, byte[] Key, byte[] IV, string Id);

    public class Handler : IRequestHandler<Request, Response>
    {
        public IEncryptionService EncryptionService;
        public IStorageService StorageService;
        public LoggingManager logging;

        public Handler(IEncryptionService encryptionService, IStorageService storageService, LoggingManager loggingManager)
        {
            EncryptionService = encryptionService;
            StorageService = storageService;
            logging = loggingManager;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            if (request.userInputDto.OIDSender == "" && request.userInputDto.OIDReceiver == "")
            {
                logging.LogEvent($"A request to create a secret for an anonymous user has been received. ID: '{request.userInputDto.Id}'.");
            }
            else
            {
                logging.LogEvent($"A request to create a secret for sender with OID '{request.userInputDto.OIDSender}' to recipient with OID '{request.userInputDto.OIDReceiver}' has been received. ID: '{request.userInputDto.Id}'.");
            }

            (byte[], byte[], byte[]) encryptionResponse;
            string ciphertext;
            UserSecret userSecret;
            string hashSender = "";
            string hashReceiver = "";

            // Hash OIDs for sender and receiver, if user is authenticated when creating secret
            if (request.userInputDto.OIDSender != "" && request.userInputDto.OIDReceiver != "")
            {
                hashSender = HashingHelper.GetHash(request.userInputDto.OIDSender);
                hashReceiver = HashingHelper.GetHash(request.userInputDto.OIDReceiver);
                logging.LogTrace($"Sender and receiver OIDs hashed successfully.");
            }

            // Encrypt data
            encryptionResponse = await EncryptionService.EncryptSecret(request.userInputDto.SecretPlaintext);
            ciphertext = System.Convert.ToBase64String(encryptionResponse.Item1);
            logging.LogTrace($"Secret succesfully encrypted. ID: '{request.userInputDto.Id}'.");

            // Create UserSecret object 
            try
            {
                UserInputDto userInputDto = request.userInputDto;
                userSecret = new UserSecret(userInputDto.Id, ciphertext, userInputDto.Lifetime, userInputDto.TimesStamp, encryptionResponse.Item2, encryptionResponse.Item3, hashSender, hashReceiver);
            }
            catch (Exception ex)
            {
                logging.LogException($"Error creating UserSecret object: '{ex.Message}'");
                throw new Exception(ex.Message, ex);
            }

            // Store secret and key
            var storageResponse = await StorageService.StoreSecret(userSecret);
            if (!storageResponse.Item1)
            {
                logging.LogException($"Storage of secret failed. ID: '{request.userInputDto.Id}'.");
                throw new Exception("Problems with storing");
            }
            logging.LogEvent($"Secret successfully stored for anonymous user. ID: '{request.userInputDto.Id}'.");

            return new Response(encryptionResponse.Item1, encryptionResponse.Item2, encryptionResponse.Item3, userSecret.Id.ToString());
        }
    }

}
