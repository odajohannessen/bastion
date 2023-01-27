namespace Bastion.Core.Domain.Encryption;

public class UserSecret // TODO: Inherit from BaseEntity? Create base entity? Should it be public?
{
    public UserSecret()
    {
    }

    public UserSecret(Guid id, string ciphertext, int inputLifetime, DateTime timeStamp)
    {
        Id = id;
        TimeStamp = timeStamp;
        Ciphertext = ciphertext;
        Lifetime = inputLifetime;
    }

    public Guid Id { get; protected set; } // TODO: Do we need to worry about how this is randomly set? Protected get? 
    // TODO: Can the guid be the url? If so, it needs to be different from UserInput ID?
    public DateTime TimeStamp { get; protected set; }
    public string Ciphertext { get; protected set; } // TODO: Protected get and protected set? 
    public int Lifetime { get; protected set; } // TODO: Protected get and protected set? 
}

