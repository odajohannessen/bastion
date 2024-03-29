﻿using MediatR;
using Bastion.Core.Domain.Encryption;
using Bastion.Core.Domain.Encryption.Services;
using Bastion.Core.Domain.UserInputSecret.Dto;
using Bastion.Managers;
using System.Text;
using Bastion.Helpers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Graph;
using System.Security.Cryptography;

namespace Bastion.Core.Domain.Encryption.Pipelines;

public class EncryptAndSaveSecret
{
    public record Request(UserInputDto userInputDto) : IRequest<Response>; 
    public record Response(bool success, string Id);

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
            bool success = true;
            (byte[], byte[], byte[]) encryptionResponse;
            string ciphertext;
            UserSecret userSecret;
            string sender = request.userInputDto.OIDSender;
            string[]? receiver = request.userInputDto.OIDReceiver;
            string receivers = "";

            if (request.userInputDto.OIDSender.IsNullOrEmpty() && request.userInputDto.OIDReceiver.IsNullOrEmpty())
            {
                logging.LogEvent($"A request to create a secret for an anonymous user has been received. ID: '{request.userInputDto.Id}'.");
            }
            else
            {
                // Hash OIDs for sender and receiver, if user is authenticated when creating secret
                //sender = HashingHelper.GetHash(request.userInputDto.OIDSender);
                //receiver = HashingHelper.GetHash(request.userInputDto.OIDReceiver);
                //logging.LogTrace($"Sender and receiver OIDs hashed successfully.");

                receivers = string.Join(", ", request.userInputDto.OIDReceiver);
                logging.LogEvent($"A request to create a secret for sender with OID '{request.userInputDto.OIDSender}' to recipient with OIDs '{receivers}' has been received. ID: '{request.userInputDto.Id}'.");
            }

            // Encrypt data
            encryptionResponse = await EncryptionService.EncryptSecret(request.userInputDto.SecretPlaintext);
            ciphertext = System.Convert.ToBase64String(encryptionResponse.Item1);
            logging.LogTrace($"Secret succesfully encrypted. ID: '{request.userInputDto.Id}'.");

            // Create UserSecret object 
            try
            {
                UserInputDto userInputDto = request.userInputDto;
                userSecret = new UserSecret(userInputDto.Id, ciphertext, userInputDto.Lifetime, userInputDto.TimesStamp, encryptionResponse.Item2, encryptionResponse.Item3, sender, receiver);
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
                success = false;
            }

            if (success && request.userInputDto.OIDSender.IsNullOrEmpty() && request.userInputDto.OIDReceiver.IsNullOrEmpty())
                logging.LogEvent($"Secret successfully stored for anonymous user. ID: '{request.userInputDto.Id}'.");
            else if (success && !request.userInputDto.OIDSender.IsNullOrEmpty() && !request.userInputDto.OIDReceiver.IsNullOrEmpty())
                logging.LogEvent($"Secret successfully stored for sender with OID '{request.userInputDto.OIDSender}' to recipient with OIDs '{receivers}' has been received.ID: '{request.userInputDto.Id}");

            return new Response(success, userSecret.Id.ToString());
        }
    }

}
