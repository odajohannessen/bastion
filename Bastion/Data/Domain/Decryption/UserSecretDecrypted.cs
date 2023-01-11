﻿namespace Bastion.Data.Domain.Decryption;

public class UserSecretDecrypted
{
    public UserSecretDecrypted(Guid id, string secretDecrypted)
    {
        Id = id;
        SecretDecrypted = secretDecrypted;
    }
    // TODO: Do we need time stamp or lifetime here? Or just ID for continuity?
    public Guid Id { get; protected set; } // TODO: Do we need to worry about how this is randomly set? Protected get? 
    public string SecretDecrypted { get; protected set; } // TODO: Protected get and protected set? 
}
