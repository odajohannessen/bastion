using System.Security.Cryptography;

namespace Bastion.Core.Domain.Decryption.Services;

public class DecryptionService : IDecryptionService
{
    public async Task<string> DecryptSecret(byte[] ciphertext) // TODO: Does this need to be a task?
    {
        // TODO: Should we implement some logging here? Security risks? 
        // TODO: Where to store logs? Blob? 
        // TODO: Which Aes class to use? Gcm? Cng? Document choice
        using (Aes aes = Aes.Create())
        {
            string decryptedData = DecryptStringFromBytes(ciphertext, aes.Key, aes.IV);
            return decryptedData; // TODO: Should return the secret

        }
    }

    private static string DecryptStringFromBytes(byte[] ciphertext, byte[] Key, byte[] IV)
    {
        // Check validity of input
        if (ciphertext == null || ciphertext.Length <= 0) 
        {
            throw new ArgumentNullException(nameof(ciphertext));       
        }
        else if (Key == null || Key.Length <= 0) 
        {
            throw new ArgumentNullException(nameof(Key));
        }
        else if (IV == null || IV.Length <= 0) 
        {
            throw new ArgumentNullException(nameof(IV));
        }

        string plaintext = "";

        try
        {
            using(Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(ciphertext))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            plaintext = sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (Exception ex) 
        {
            // TODO: Logging?
            throw new Exception(ex.ToString());
        }

        return plaintext;
    }
}
