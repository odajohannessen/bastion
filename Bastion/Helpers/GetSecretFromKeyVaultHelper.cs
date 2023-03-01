using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure;

namespace Bastion.Helpers;

public class GetSecretFromKeyVaultHelper
{
    // Access key vault and retrieve a secret
    public static string GetSecret(string secretName)
    {
        Response<KeyVaultSecret> secret;
        string keyVaultName = "kvbastion-secrets";
        var uri = $"https://{keyVaultName}.vault.azure.net/";
        var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
        SecretClient client = new SecretClient(new Uri(uri), credentials);

        try 
        {
            secret = client.GetSecret(secretName);
        }
        catch 
        {
            return "Secret not found";
        }
        return secret.Value.Value.ToString();
    }
}
