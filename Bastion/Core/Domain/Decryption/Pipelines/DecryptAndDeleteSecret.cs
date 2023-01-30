using MediatR;
using Bastion.Core.Domain.Decryption;
using Bastion.Core.Domain.Decryption.Services;

namespace Bastion.Core.Domain.Decryption.Pipelines;

public class DecryptAndDeleteSecret
{
    // TODO: What will be input here? String from storage? byte? 
    public record Request(byte[] ciphertextBytes, byte[] key, byte[] IV) : IRequest<Response>;
    public record Response(string Plaintext);
    // TODO: Id as input?
    // TODO: Delete secret from storage in this pipeline
    // TODO: Rename pipeline? DecryptAndDeleteSecret? 

    public class Handler : IRequestHandler<Request, Response>
    {
        public IDecryptionService DecryptionService;

        public Handler(IDecryptionService decryptionService)
        {
            DecryptionService = decryptionService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            string plaintext;

            try
            {
                // TODO: Fetch secret and key from URL ID 
                plaintext = await DecryptionService.DecryptSecret(request.ciphertextBytes, request.key, request.IV); // Also need key, need IV?
                // TODO: Delete secret
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message, ex);
                // TODO: Fix issue with padding
            }

            return new Response(plaintext); // TODO: Return bool?

        }
    }


}
