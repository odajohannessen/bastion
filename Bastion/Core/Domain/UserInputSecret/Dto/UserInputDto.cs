﻿namespace Bastion.Core.Domain.UserInputSecret.Dto;

public record UserInputDto(Guid Id, DateTime TimesStamp, string SecretPlaintext, int Lifetime, string OIDSender = "", string[]? OIDReceiver = null);