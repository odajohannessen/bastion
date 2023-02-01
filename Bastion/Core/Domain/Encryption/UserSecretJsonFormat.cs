namespace Bastion.Core.Domain.Encryption;

public class UserSecretJsonFormat
{
    public UserSecretJsonFormat(Guid id, string ciphertext, int inputLifetime, DateTime timeStamp, byte[] iv)
    {
        Id = id;
        TimeStamp = timeStamp;
        Ciphertext = ciphertext; // TODO: Change this to byte[]?
        Lifetime = inputLifetime;
        IV = iv;
    }

    public Guid Id { get; protected set; } // TODO: Do we need to worry about how this is randomly set? Protected get? 
    public DateTime TimeStamp { get; protected set; }
    public string Ciphertext { get; protected set; } // TODO: Protected get and protected set? 
    public int Lifetime { get; protected set; } // TODO: Protected get and protected set
    public byte[] IV { get; protected set; }
}
