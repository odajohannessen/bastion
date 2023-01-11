namespace Bastion.Data.Domain.Encryption;

public class UserSecret // TODO: Inherit from BaseEntity? Create base entity? Should it be public?
{
    public UserSecret(Guid id, string inputSecretEncrypted, int inputLifeTime, DateTime timeStamp)
    {
        Id = id;
        TimeStamp = timeStamp;
        SecretEncrypted = inputSecretEncrypted;
        LifeTime = inputLifeTime;
    }

    public Guid Id { get; protected set; } // TODO: Do we need to worry about how this is randomly set? Protected get? 
    // TODO: Can the guid be the url? If so, it needs to be different from UserInput ID?
    public DateTime TimeStamp { get; protected set; }
    public string SecretEncrypted { get; protected set; } // TODO: Protected get and protected set? 
    public int LifeTime { get; protected set; } // TODO: Protected get and protected set? 
}

