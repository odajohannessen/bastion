﻿namespace Bastion.Data.Domain.UserInput;

public class UserInput // TODO: Inherit from BaseEntity? Create base entity? 
{
    public UserInput(string inputSecretPlainText, int inputLifeTime)
    {
        Id = Guid.NewGuid();
        TimeStamp = DateTime.Now;
        SecretPlainText = inputSecretPlainText;
        LifeTime= inputLifeTime;
    }
    // TODO: Can we use constructor like this to create an object when taking in input?
    // TODO: Separate input form class model? 
    public Guid Id { get; protected set; } // TODO: Do we need to worry about how this is randomly set?
    public DateTime TimeStamp { get; protected set; }
    public string SecretPlainText { get; protected set; } // TODO: Protected get and protected set? 
    public int LifeTime { get; protected set; } // TODO: Protected get and protected set? 
}
