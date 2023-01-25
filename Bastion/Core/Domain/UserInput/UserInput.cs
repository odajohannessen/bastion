namespace Bastion.Core.Domain.UserInput;

public class UserInput // TODO: Inherit from BaseEntity? Create base entity? 
{
    public UserInput(string inputSecretPlainText, int inputLifeTime)
    {
        Id = Guid.NewGuid();
        TimeStamp = DateTime.Now;
        SecretPlaintext = inputSecretPlainText;
        Lifetime= inputLifeTime;
    }
    // TODO: Can we use constructor like this to create an object when taking in input?
    // TODO: Separate input form class model? 
    // Bind form values in the code window in razor page? Use it to create object perhaps? Need it to get Id 
    public Guid Id { get; protected set; } // TODO: Do we need to worry about how this is randomly set?
    public DateTime TimeStamp { get; protected set; }
    public string SecretPlaintext { get; protected set; } // TODO: Protected get and protected set? 
    public int Lifetime { get; protected set; } // TODO: Protected get and protected set? 
}
