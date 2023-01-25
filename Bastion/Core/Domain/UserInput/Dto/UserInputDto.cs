namespace Bastion.Core.Domain.UserInput.Dto;

public record UserInputDto(Guid Id, DateTime TimesStamp, string SecretPlaintext, int Lifetime);