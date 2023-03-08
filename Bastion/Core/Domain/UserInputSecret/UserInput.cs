namespace Bastion.Core.Domain.UserInputSecret;

public class UserInput 
{
    public UserInput(string inputSecretPlainText, int inputLifeTime, string oidSender="", string oidReceiver="")
    {
        Id = Guid.NewGuid();
        TimeStamp = DateTime.Now;
        Plaintext = inputSecretPlainText;
        Lifetime = inputLifeTime;
        OIDSender = oidSender;
        OIDReceiver = oidReceiver;
    }

    public Guid Id { get; protected set; } 
    public DateTime TimeStamp { get; protected set; }
    public string Plaintext { get; protected set; } 
    public int Lifetime { get; protected set; }
    public string OIDSender { get; protected set; }
    public string OIDReceiver { get; protected set; }
}