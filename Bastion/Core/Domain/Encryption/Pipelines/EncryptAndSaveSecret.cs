using MediatR;
using Bastion.Core.Domain.Encryption;
using Bastion.Core.Domain.Encryption.Services;
using Bastion.Core.Domain.UserInput.Dto;

namespace Bastion.Core.Domain.Encryption.Pipelines;

public class EncryptAndSaveSecret
{
    public record Request(UserInputDto userInputDto) : IRequest<Response>; 
    public record Response(byte[] ciphertextBytes);

    public class Handler : IRequestHandler<Request, Response>
    {
        public IEncryptionService EncryptionService;

        public Handler(IEncryptionService encryptionService) 
        {
            EncryptionService = encryptionService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            byte[] ciphertextBytes; 

            try
            {
                // Encrypt data
                ciphertextBytes = await EncryptionService.EncryptSecret(request.userInputDto.SecretPlaintext);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            // Create UserSecret object 
            UserInputDto userInputDto = request.userInputDto;
            UserSecret userSecret =  new UserSecret(userInputDto.Id, userInputDto.SecretPlaintext, userInputDto.Lifetime, userInputDto.TimesStamp);
            // TODO: Try catch here of user secret
            // TODO: Need to return anything else here except bool?
            // TODO: Call on storage service here. Secret ID is blob name
            // TODO: Where should lifetime be stored? Separate blob id-lifetime name? 
            // TODO: We also need to store key with secret ID in KV
            // URL = guid 

            return new Response(ciphertextBytes);
        }
    }

}
