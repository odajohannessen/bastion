namespace Bastion.Core.Domain.Encryption;

public class UserSecretJsonFormat
{
    public UserSecretJsonFormat(Guid id, string ciphertext, int inputLifetime, DateTime timeStamp, byte[] iv, string oidSender = "")
    {
        Id = id;
        TimeStamp = timeStamp;
        Lifetime = inputLifetime;
        ExpireTimeStamp = timeStamp.AddHours(inputLifetime);
        Ciphertext = ciphertext;
        IV = iv;
        OIDSender = oidSender;
    }

    public Guid Id { get; protected set; } 
    public DateTime TimeStamp { get; protected set; }
    public int Lifetime { get; protected set; } 
    public DateTime ExpireTimeStamp { get; protected set; }
    public string Ciphertext { get; protected set; } 
    public byte[] IV { get; protected set; }
    public string OIDSender { get; protected set; }
}
