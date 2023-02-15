using System.Security.Cryptography;

namespace Bastion.Core.Domain.Encryption.Services;

public class EncryptionService : IEncryptionService
{
    public async Task<(byte[], byte[], byte[])> EncryptSecret(string plaintext) 
    {
        using (Aes aes = Aes.Create())
        {
            var encryptionResponse = EncryptStringToBytes(plaintext, aes.Key, aes.IV);
            return encryptionResponse;
        }
    }

    private static (byte[], byte[], byte[]) EncryptStringToBytes(string plaintext, byte[] Key, byte[] IV)
    {
        // Check validity of input
        if (plaintext == null || plaintext.Length <= 0) 
        {
            throw new ArgumentNullException(nameof(plaintext));       
        }
        else if (Key == null || Key.Length <= 0) 
        {
            throw new ArgumentNullException(nameof(Key));
        }
        else if (IV == null || IV.Length <= 0) 
        {
            throw new ArgumentNullException(nameof(IV));
        }

        // Byte array
        byte[] ciphertextBytes;

        try
        {
            // AES object with key and initialization vector (IV)
            using (Aes aes = Aes.Create())
            {
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Key;
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
                        ciphertextBytes = ms.ToArray(); 
                    }
                }

                return (ciphertextBytes, aes.Key, aes.IV);

            }

        }
        catch (Exception ex) 
        {
            // TODO: Logging?
            throw new Exception(ex.ToString());
        }
    }
}
