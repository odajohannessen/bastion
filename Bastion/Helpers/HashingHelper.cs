using System;
using System.Text;
using System.Security.Cryptography;

namespace Bastion.Helpers;

public class HashingHelper
{
    // Compare a value to a hash, returns true if they match, false if not
    public static bool VerifyHash(string value, string hash)
    {
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        string hashOfValue;

        using (SHA256 sha256Hash = SHA256.Create())
        {
            hashOfValue = GetHash(value);
        }

        return comparer.Compare(hashOfValue, hash) == 0;
    }

    // Hash an input value
    public static string GetHash(string value)
    {
        var sBuilder = new StringBuilder();

        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(value));

            // Loop through each byte of the hashed data and format each as a hexadecimal string
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
        }

        return sBuilder.ToString();
    }
}
