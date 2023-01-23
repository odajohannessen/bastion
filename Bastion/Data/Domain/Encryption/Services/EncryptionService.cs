using System.Security.Cryptography;

namespace Bastion.Data.Domain.Encryption.Services;

public class EncryptionService : IEncryptionService
{
    public async Task<bool> EncryptSecret(string plaintext)
    {
        // TODO: Should we implement some logging here? Security risks?
        // TODO: Which Aes class to use? Gcm? Cng? Document choice
        string plaintextTest = "Lets encrypt this";
        using (Aes aes = Aes.Create())
        {
            byte[] encryptedData = EncryptStringToBytes(plaintextTest, aes.Key, aes.IV);

        }

        return true;
    }

    private static byte[] EncryptStringToBytes(string plaintext, byte[] key, byte[] IV)
    {
        // Check validity of input
        if (plaintext == null || plaintext.Length <= 0) 
        {
            throw new ArgumentNullException(nameof(plaintext));       
        }
        else if (key == null || key.Length <= 0) 
        {
            throw new ArgumentNullException(nameof(key));
        }
        else if (IV == null || IV.Length <= 0) 
        {
            throw new ArgumentNullException(nameof(IV));
        }

        // Byte array
        byte[] encryptedData;

        try
        {
            // AES object with key and initialization vector (IV)
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;

                // Create encryptor to transform input plaintext to byte array ciphertext
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // TODO: Any difference to this than doing using? Okay?
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                StreamWriter sw = new StreamWriter(cs);
                sw.Write(plaintext);
                encryptedData = ms.ToArray();

                return encryptedData;
            }

        }
        catch (Exception ex) 
        {
            // TODO: Logging?
            throw new Exception(ex.ToString());
        }
    }
}
