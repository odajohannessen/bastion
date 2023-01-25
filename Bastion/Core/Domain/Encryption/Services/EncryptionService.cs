using System.Security.Cryptography;

namespace Bastion.Core.Domain.Encryption.Services;

public class EncryptionService : IEncryptionService
{
    public async Task<byte[]> EncryptSecret(string plaintext) // TODO: Does this need to be a task?
    {
        // TODO: Should we implement some logging here? Security risks? 
        // TODO: Where to store logs? Blob? 
        // TODO: Which Aes class to use? Gcm? Cng? Document choice
        using (Aes aes = Aes.Create())
        {
            byte[] encryptedData = EncryptStringToBytes(plaintext, aes.Key, aes.IV);
            return encryptedData; // TODO: Should return the secret

        }
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

                // Streams for encryption
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plaintext);
                        }
                        encryptedData = ms.ToArray(); 
                    }
                }

                return encryptedData;

                // TODO: Unit test for encrypt and decrypt
                // TODO: Choice of key length? Performance vs security
            }

        }
        catch (Exception ex) 
        {
            // TODO: Logging?
            throw new Exception(ex.ToString());
        }
    }
}
