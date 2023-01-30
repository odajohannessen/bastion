using MediatR;
using Bastion.Core.Domain.Encryption;
using Bastion.Core.Domain.Encryption.Services;
using Bastion.Core.Domain.UserInput.Dto;
using Bastion.Core.Domain.Decryption.Pipelines; // For testing
using Bastion.Core.Domain.Decryption.Services;

namespace Bastion.Core.Domain.Encryption.Pipelines;

public class EncryptAndSaveSecret
{
    public record Request(UserInputDto userInputDto) : IRequest<Response>; 
    public record Response(byte[] ciphertextBytes, byte[] key, byte[] IV);

    public class Handler : IRequestHandler<Request, Response>
    {
        public IEncryptionService EncryptionService;

        public Handler(IEncryptionService encryptionService) 
        {
            EncryptionService = encryptionService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            (byte[], byte[], byte[]) encryptionResponse;
            string ciphertext;

            try
            {
                // Encrypt data
                encryptionResponse = await EncryptionService.EncryptSecret(request.userInputDto.SecretPlaintext);
                ciphertext = System.Text.Encoding.Default.GetString(encryptionResponse.Item1);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            // Create UserSecret object 
            UserInputDto userInputDto = request.userInputDto;
            UserSecret userSecret =  new UserSecret(userInputDto.Id, ciphertext, userInputDto.Lifetime, userInputDto.TimesStamp, encryptionResponse.Item2, encryptionResponse.Item3);
            
            // TODO: Try catch here of user secret
            // TODO: Need to return anything else here except bool?
            // TODO: Call on storage service here. Secret ID is blob name
            // TODO: Where should lifetime be stored? Separate blob id-lifetime name? 
            // TODO: We also need to store key with secret ID in KV
            // TODO: URL = guid 

            return new Response(encryptionResponse.Item1, encryptionResponse.Item2, encryptionResponse.Item3);
        }
    }

}
