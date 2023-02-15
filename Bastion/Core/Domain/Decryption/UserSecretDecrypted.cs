namespace Bastion.Core.Domain.Decryption;

public class UserSecretDecrypted
{
    public UserSecretDecrypted(Guid id, string plaintext)
    {
        Id = id;
        Plaintext = plaintext;
    }
    public Guid Id { get; protected set; } 
    public string Plaintext { get; protected set; } 
}
