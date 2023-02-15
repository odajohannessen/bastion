namespace Bastion.Core.Domain.UserInputSecret;

public class UserInput 
{
    public UserInput(string inputSecretPlainText, int inputLifeTime)
    {
        Id = Guid.NewGuid();
        TimeStamp = DateTime.Now;
        Plaintext = inputSecretPlainText;
        Lifetime= inputLifeTime;
    }

    public Guid Id { get; protected set; } 
    public DateTime TimeStamp { get; protected set; }
    public string Plaintext { get; protected set; } 
    public int Lifetime { get; protected set; } 
}