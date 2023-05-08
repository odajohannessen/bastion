using Bastion.Managers;
using System.Security.Cryptography;

namespace Bastion.Core.Domain.Decryption.Services;

public class DecryptionService : IDecryptionService
{
    public LoggingManager logging;

    public DecryptionService(LoggingManager loggingManager)
    {
        logging = loggingManager;
    }

    public async Task<string> DecryptSecret(byte[] ciphertextBytes, byte[] key, byte[] IV) 
    {
        string plaintext = DecryptStringFromBytes(ciphertextBytes, key, IV);
        return plaintext;
  
    }

    private string DecryptStringFromBytes(byte[] ciphertext, byte[] Key, byte[] IV)
    {
        // Check validity of input
        if (ciphertext == null || ciphertext.Length <= 0) 
        {
            logging.LogException("Invalid ciphertext input.");
            throw new ArgumentNullException(nameof(ciphertext));       
        }
        else if (Key == null || Key.Length <= 0) 
        {
            logging.LogException("Invalid key input.");
            throw new ArgumentNullException(nameof(Key));
        }
        else if (IV == null || IV.Length <= 0) 
        {
            logging.LogException("Invalid IV input.");
            throw new ArgumentNullException(nameof(IV));
        }

        string plaintext;

        try
        {
            using(Aes aes = Aes.Create())
            {
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Key;
                aes.IV = IV;

                // Create an encryptor to perform the stream transform
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
            logging.LogException($"Error decypting ciphertext: '{ex.Message}'.");
            throw new Exception(ex.ToString());
        }

        return plaintext;
    }
}
