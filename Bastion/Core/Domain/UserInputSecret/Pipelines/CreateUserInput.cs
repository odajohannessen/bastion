using MediatR;
using Microsoft.EntityFrameworkCore;
using Bastion.Core.Domain.UserInputSecret;

namespace Bastion.Core.Domain.UserInputSecret.Pipelines;

public class CreateUserInput
{
    public record Request(string inputSecretPlaintext, int inputLifeTime) : IRequest<Response>;

    public record Response(bool success, UserInput userInput);

    public class Handler : IRequestHandler<Request, Response> 
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken) // TODO: What do we need cancellation token for? 
        {
            try
            {
                // Create a new UserInput object
                UserInput userInput = new UserInput(request.inputSecretPlaintext, request.inputLifeTime);
                return new Response(true, userInput);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
